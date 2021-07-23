using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
namespace LostPolygon.SpriteSharp.Utility.Internal {
    /// <summary>
    /// Allows to easily extend upon existing Editor without actually inheriting it.
    /// </summary>
    internal abstract class EditorDecorator : Editor {
        private static readonly Action<Editor> _onHeaderGUIDelegate;

        [SerializeField]
        private Editor _decoratedEditor;

        private Type _decoratedEditorType;
        private Action<Editor> _onSceneGUIDelegate;

        protected abstract Type DecoratedEditorType { get; }

        protected Editor DecoratedEditor {
            get {
                return _decoratedEditor;
            }
        }

        static EditorDecorator() {
            MethodInfo onHeaderGUIMethodInfo = typeof(Editor).GetMethod("OnHeaderGUI", BindingFlags.NonPublic | BindingFlags.Instance);
            _onHeaderGUIDelegate = (Action<Editor>) Delegate.CreateDelegate(typeof(Action<Editor>), onHeaderGUIMethodInfo);
        }

        #region Overrides

        public override void OnInspectorGUI() {
            CallDecoratedOnInspectorGUI();
        }

        public override bool HasPreviewGUI() {
            UpdateDecoratedEditorState();
            return _decoratedEditor.HasPreviewGUI();
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background) {
            UpdateDecoratedEditorState();
            _decoratedEditor.OnPreviewGUI(rect, background);
        }

        public override string GetInfoString() {
            return _decoratedEditor.GetInfoString();
        }

        public override bool RequiresConstantRepaint() {
            return _decoratedEditor.RequiresConstantRepaint();
        }

        public override GUIContent GetPreviewTitle() {
            return _decoratedEditor.GetPreviewTitle();
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) {
            return _decoratedEditor.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        public override void OnPreviewSettings() {
            _decoratedEditor.OnPreviewSettings();
        }

        public override void ReloadPreviewInstances() {
            _decoratedEditor.ReloadPreviewInstances();
        }

        public override bool UseDefaultMargins() {
            return _decoratedEditor.UseDefaultMargins();
        }

        public override string ToString() {
            return _decoratedEditor.ToString();
        }

        protected override void OnHeaderGUI() {
            UpdateDecoratedEditorState();
            _onHeaderGUIDelegate(_decoratedEditor);
        }

        #endregion

        #region Unity methods

        protected virtual void OnEnable() {
            CreateDecoratedInspector();
        }

        protected virtual void OnDisable() {
            DestroyDecoratedInspector();
        }

        #endregion

        #region Decorated object manipulation

        protected void CallDecoratedOnInspectorGUI() {
            if (_decoratedEditor == null) {
                CreateDecoratedInspector();
            }

            if (_decoratedEditor != null) {
                _decoratedEditor.OnInspectorGUI();
            } else {
                Debug.LogError("_decoratedEditor == null");
            }
        }

        protected virtual void UpdateDecoratedEditorState() {
            _decoratedEditor.SetReferenceTargetIndex(this.GetReferenceTargetIndex());
            //_decoratedEditor.UpdateReferenceTargetIndex(DecoratedEditorType);
        }

        protected void CallDecoratedOnSceneGUI() {
            _onSceneGUIDelegate(_decoratedEditor);
        }

        protected void CreateDecoratedInspector() {
            DestroyDecoratedInspector();

            if (_decoratedEditorType == null) {
                _decoratedEditorType = DecoratedEditorType;
                MethodInfo onSceneGUIMethodInfo = _decoratedEditorType.GetMethod("OnSceneGUI", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (onSceneGUIMethodInfo != null) {
                    _onSceneGUIDelegate = (Action<Editor>) Delegate.CreateDelegate(typeof(Action<Editor>), onSceneGUIMethodInfo);
                }
            }

            _decoratedEditor = CreateEditor(targets.Where(o => o != null).ToArray(), _decoratedEditorType);
        }

        protected void DestroyDecoratedInspector() {
            if (_decoratedEditor != null) {
                DestroyImmediate(_decoratedEditor);
            }
        }

        #endregion
    }
}