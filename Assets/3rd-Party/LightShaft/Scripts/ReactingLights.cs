using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class ReactingLights : MonoBehaviour {

	public VideoPlayer videoSource;
	public Light[] lights;
	public Color averageColor;
	private Texture2D tex;

	public enum VideoSide{
		up,
		left,
		right,
		down,
		center
	}

	public VideoSide videoSide;

	private void Start(){
		videoSource.frameReady += NewFrame;
		videoSource.sendFrameReadyEvents = true;
	} 

	bool createTexture = false;
	private void NewFrame(VideoPlayer vplayer, long frame){
		if (!createTexture) {
			createTexture = true;
			switch (videoSide) {
			case VideoSide.up:
				tex = new Texture2D(videoSource.texture.width/2,20);
				break;
			case VideoSide.down:
				tex = new Texture2D(videoSource.texture.width/2,20);
				break;
			case VideoSide.left:
				tex = new Texture2D(20,videoSource.texture.height/2);
				break;
			case VideoSide.right:
				tex = new Texture2D(20,videoSource.texture.height/2);
				break;
			case VideoSide.center:
				tex = new Texture2D (videoSource.texture.height / 2, videoSource.texture.height / 2);
				break;
			}
		}
		RenderTexture.active = (RenderTexture)videoSource.texture;
		switch (videoSide) {
			case VideoSide.up:
			tex.ReadPixels(new Rect((videoSource.texture.width/2),0,videoSource.texture.width/2,20),0,0);
				break;
			case VideoSide.down:
			tex.ReadPixels(new Rect((videoSource.texture.width/2),videoSource.texture.height-20,videoSource.texture.width/2,20),0,0);
				break;
			case VideoSide.left:
			tex.ReadPixels(new Rect(0,0,20,videoSource.texture.height/2),0,0);
				break;
			case VideoSide.right:
			tex.ReadPixels(new Rect(videoSource.texture.width-20,0,20,videoSource.texture.height/2),0,0);
				break;
			case VideoSide.center:
				tex.ReadPixels(new Rect((videoSource.texture.width/2)-(videoSource.texture.width/2),(videoSource.texture.height/2)-(videoSource.texture.height/2),videoSource.texture.width/2,videoSource.texture.height/2),0,0);
				break;
		}

		tex.Apply();
		averageColor = AverageColorFromTexture (tex);
		averageColor.a = 1;
		foreach (Light light in lights)
			light.color = averageColor;
	}

	Color32 AverageColorFromTexture(Texture2D tex)
	{

		Color32[] texColors = tex.GetPixels32();

		int total = texColors.Length;

		float r = 0;
		float g = 0;
		float b = 0;

		for(int i = 0; i < total; i++)
		{

			r += texColors[i].r;

			g += texColors[i].g;

			b += texColors[i].b;

		}

		return new Color32((byte)(r / total) , (byte)(g / total) , (byte)(b / total) , 0);

	}
}
