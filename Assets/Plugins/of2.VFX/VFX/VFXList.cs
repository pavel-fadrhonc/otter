using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXList
{
	/// VFX_LIST_START

	public enum EVFX {
		NONE = 0,
		HIT = 1063196330,
	}

	public static string Get(EVFX vfx){
	string vfxId = null;
	switch(vfx){
		case EVFX.HIT:
			vfxId = "Hit";
			break;
	}
	return vfxId;
	}
/// VFX_LIST_END
}
