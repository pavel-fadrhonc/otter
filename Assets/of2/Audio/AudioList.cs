// This file is autogenerated by AudioManager!


using of2.Audio;
public class AudioList {
	public enum Sounds {
		NONE = -1,
		AAAH = 1912152884,
		AU = 269152545,
		CACHT = 1815625119,
		CHARACTERS = 4,
		MUSIC = 2,
		VO = 5,
	}

}

public class ProjectSpecificAudioHelper : IAudioManagerHelper{
	public string AudioEnumToStringId(int enumAsInt){
		AudioList.Sounds soundEnum = (AudioList.Sounds) enumAsInt;
		string soundIdStr = null;
		  switch(soundEnum){
		case AudioList.Sounds.AAAH:
			soundIdStr = "Aaah";
			break;
		case AudioList.Sounds.AU:
			soundIdStr = "AU";
			break;
		case AudioList.Sounds.CACHT:
			soundIdStr = "cacht";
			break;
		case AudioList.Sounds.CHARACTERS:
			soundIdStr = "Characters";
			break;
		case AudioList.Sounds.MUSIC:
			soundIdStr = "Music";
			break;
		case AudioList.Sounds.VO:
			soundIdStr = "VO";
			break;
		}
		return soundIdStr;
	}
}