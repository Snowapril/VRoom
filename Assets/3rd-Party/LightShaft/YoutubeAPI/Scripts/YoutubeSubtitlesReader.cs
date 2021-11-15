using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Networking;

public class YoutubeSubtitlesReader : MonoBehaviour
{

    private readonly string[] _delimiters = new string[] { "-->", "- >", "->" };
    public YoutubeSubtitlesReader() { }
    public string videoID;

    /*SOME LANG CODES
     * 
     * Englsh = en
     * Português Brasil = pt_BR 
     * Português Portugal = pt
     * French = fr
     * you can search for lang codes if you dont know how code to use.
     * https://developers.google.com/admin-sdk/directory/v1/languages
     * 
     */

    public string langCode;
    public VideoPlayer videoPlayer;
    bool subtitleLoaded = false;
    public string currentTextLine;
    public Text uiSubtitle;
    List<SubtitleItem> subtitleList;

    private void Start()
    {
        LoadSubtitle();
    }


    public void LoadSubtitle()
    {
        subtitleLoaded = false;
        StartCoroutine(DownloadSubtitle());
    }
    private void WhenSubtitleLoadAreReady(List<SubtitleItem> result)
    {
        subtitleList = result;
        Debug.Log("Subtitle Loaded");
        //Debug.Log(result[0].StartTime +" "+ result[0].EndTime);
        subtitleLoaded = true;
    }

    System.Collections.IEnumerator DownloadSubtitle()
    {
        //This is a url was made to use with this plugin only, please dont share it.
        UnityWebRequest request = UnityWebRequest.Get("https://lightshaftstream.herokuapp.com/api/subtitle?url=https://www.youtube.com/watch?v="+videoID+"");
        //request.SetRequestHeader("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:10.0) Gecko/20100101 Firefox/10.0 (Chrome)");
        Debug.Log(request.url);
        yield return request.SendWebRequest();
        JSONNode subtitleList = JSON.Parse(request.downloadHandler.text);
        JSONNode lang = subtitleList["subtitles"][0][langCode];
        if (lang.Count > 0)
        {
            for (int x = 0; x < lang.Count; x++)
            {
                if (lang[x]["ext"] == "vtt")
                {
                    StartCoroutine(DownloadSubtitleFile(lang[x]["url"]));
                    break;
                }
            }
        }
    }

    System.Collections.IEnumerator DownloadSubtitleFile(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        subtitleList = ParseStream(request.downloadHandler.data);
        WhenSubtitleLoadAreReady(subtitleList);
    }

    private void FixedUpdate()
    {
        if (videoPlayer.isPlaying && subtitleLoaded)
        {
            foreach (SubtitleItem item in subtitleList)
            {
                if(videoPlayer.time >= item.StartTime  && videoPlayer.time <= item.EndTime)
                {
                    currentTextLine = item.text;
                    uiSubtitle.text = currentTextLine;
                    break;
                }
                else
                {
                    currentTextLine = "";
                    uiSubtitle.text = currentTextLine;
                }
            }
        }
    }


    public List<SubtitleItem> ParseStream(byte[] subtitleBytes)
    {
        // test if stream if readable and seekable (just a check, should be good)
        //if (!vttStream.CanRead || !vttStream.CanSeek)
        //{
        //    var message = string.Format("Stream must be seekable and readable in a subtitles parser. " +
        //                       "Operation interrupted; isSeekable: {0} - isReadable: {1}",
        //                       vttStream.CanSeek, vttStream.CanSeek);
        //    throw new ArgumentException(message);
        //}

        // seek the beginning of the stream
        //vttStream.Position = 0;

        //var reader = new StreamReader(vttStream, encoding, true);

        var items = new List<SubtitleItem>();
        var vttSubParts = GetVttSubTitleParts(subtitleBytes).ToList();
        if (vttSubParts.Any())
        {
            foreach (var vttSubPart in vttSubParts)
            {
                var lines =
                    vttSubPart.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                              .Select(s => s.Trim())
                              .Where(l => !string.IsNullOrEmpty(l))
                              .ToList();

                var item = new SubtitleItem();
                foreach (var line in lines)
                {
                    if (item.StartTime == 0 && item.EndTime == 0)
                    {
                        // we look for the timecodes first
                        double startTc;
                        double endTc;
                        var success = TryParseTimecodeLine(line, out startTc, out endTc);
                        if (success)
                        {
                            item.StartTime = startTc/1000;
                            item.EndTime = endTc/1000;
                        }
                    }
                    else
                    {
                        // we found the timecode, now we get the text
                        item.text += line + " \n";
                    }
                }

                if ((item.StartTime != 0 || item.EndTime != 0) && item.text.Any())
                {
                    // parsing succeeded
                    items.Add(item);
                }
            }

            if (items.Any())
            {
                return items;
            }
            else
            {
                throw new ArgumentException("Stream is not in a valid VTT format");
            }
        }
        else
        {
            throw new FormatException("Parsing as VTT returned no VTT part.");
        }

    }

    /// <summary>
    /// Enumerates the subtitle parts in a VTT file based on the standard line break observed between them. 
    /// A VTT subtitle part is in the form:
    /// 
    /// CUE - 1
    /// 00:00:20.000 --> 00:00:24.400
    /// Altocumulus clouds occur between six thousand
    /// 
    /// The first line is optional, as well as the hours in the time codes.
    /// </summary>
    /// <param name="reader">The textreader associated with the vtt file</param>
    /// <returns>An IEnumerable(string) object containing all the subtitle parts</returns>
    private IEnumerable<string> GetVttSubTitleParts(byte[] r)
    {
        string line;
        var sb = new StringBuilder();
        MemoryStream stream = new MemoryStream(r);
        // convert stream to string
        StreamReader reader = new StreamReader(stream);

        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrEmpty(line.Trim()))
            {
                // return only if not empty
                var res = sb.ToString().TrimEnd();
                if (!string.IsNullOrEmpty(res))
                {
                    yield return res;
                }
                sb = new StringBuilder();
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        if (sb.Length > 0)
        {
            yield return sb.ToString();
        }
    }

    private bool TryParseTimecodeLine(string line, out double startTc, out double endTc)
    {
        var parts = line.Split(_delimiters, StringSplitOptions.None);
        if (parts.Length != 2)
        {
            // this is not a timecode line
            startTc = -1;
            endTc = -1;
            return false;
        }
        else
        {
            startTc = ParseVttTimecode(parts[0]);
            endTc = ParseVttTimecode(parts[1]);
            return true;
        }
    }

    /// <summary>
    /// Takes an VTT timecode as a string and parses it into a double (in seconds). A VTT timecode reads as follows: 
    /// 00:00:20.000
    /// or
    /// 00:20.000
    /// </summary>
    /// <param name="s">The timecode to parse</param>
    /// <returns>The parsed timecode as a TimeSpan instance. If the parsing was unsuccessful, -1 is returned (subtitles should never show)</returns>
    private int ParseVttTimecode(string s)
    {
        string timeString = string.Empty;
        var match = Regex.Match(s, "[0-9]+:[0-9]+:[0-9]+[,\\.][0-9]+");
        if (match.Success)
        {
            timeString = match.Value;
        }
        else
        {
            match = Regex.Match(s, "[0-9]+:[0-9]+[,\\.][0-9]+");
            if (match.Success)
            {
                timeString = "00:" + match.Value;
            }
        }

        if (!string.IsNullOrEmpty(timeString))
        {
            timeString = timeString.Replace(',', '.');
            TimeSpan result;
            if (TimeSpan.TryParse(timeString, out result))
            {
                var nbOfMs = (int)result.TotalMilliseconds;
                return nbOfMs;
            }
        }

        return -1;
    }
}

public class SubtitleItem
{

    //Properties------------------------------------------------------------------

    //StartTime and EndTime times are in milliseconds
    public double StartTime { get; set; }
    public double EndTime { get; set; }
    public string text { get; set; }


    //Constructors-----------------------------------------------------------------

    /// <summary>
    /// The empty constructor
    /// </summary>
    public SubtitleItem()
    {
        //this.Lines = new List<string>();
    }

}