using UnityEngine;

namespace OakFramework2.BaseMono
{
    /// <summary>
    /// Base class for everything in the game. 
    /// of2GameObject inherits from this and most objects should inherit from NNGameObject. 
    /// However, managers and controllers might inherit from this class.
    /// 
    /// It cleans up its Event listeners and instanced Materials by itself.
    /// </summary>
    public class of2MonoBehaviour : MonoBehaviour
    {
	    protected virtual void Awake()
        { }

        protected virtual void Start()
        { }

        protected virtual void OnEnable()
        { }

        protected virtual void OnDisable()
        { }

        protected virtual void OnDestroy()
        {
	        CleanupInstancedMaterials();
        }

        private void CleanupInstancedMaterials()
        {
#if !UNITY_EDITOR
		Renderer r = GetComponent<Renderer>();
		if (r != null)
		{
			if (r.sharedMaterials != null && r.sharedMaterials.Length > 0)
			{
				for (int i = 0; i < r.sharedMaterials.Length; i++)
				{
					if (r.sharedMaterials[i] == null)
						continue;
					if (r.sharedMaterials[i].name.EndsWith("(Instance)"))
					{
						r.sharedMaterials[i].mainTexture = null;
						Destroy(r.sharedMaterials[i]);
					}
				}
			}
		}
		
		MeshFilter mf = gameObject.GetComponent<MeshFilter>();
		if (mf != null)
		{
			if (mf.sharedMesh != null && mf.sharedMesh.name.EndsWith("(Instance)"))
			{
				mf.sharedMesh = null;
				Destroy(mf.sharedMesh);
			}
		}
#endif
        }

        public void ForceOnDestroyCall()
        {
            OnDestroy();
        }

        public static void CleanUpNNMonoBehaviours(GameObject go)
        {
            // Since OnDestroy only gets called when destroying objects that has previously been active, we have to call the ForceOnDestroy method, to be sure we unload everything
            of2MonoBehaviour[] embs = go.GetComponentsInChildren<of2MonoBehaviour>(true);
            if (embs != null && embs.Length > 0)
            {
                for (int i = 0; i < embs.Length; i++)
                {
                    if (embs[i] != null)
                        embs[i].ForceOnDestroyCall();
                }
            }
        }
    }
}