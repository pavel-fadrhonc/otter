using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;
using Zenject;

namespace Zenject
{
    public interface ILateInitializable
    {
        void LateInitialize();
    }
    
    public class LateInitializableManager
    {
        bool _hasInitialized;        
        
        private List<LateInitializableInfo> _lateInitializables;
        private readonly CoroutineRunner _coroutineRunner;

        public LateInitializableManager(
            [Inject(Optional = true, Source = InjectSources.Local)]
            List<ILateInitializable> lateInitializables,
            [Inject(Optional = true, Source = InjectSources.Local)]
            List<ValuePair<Type, int>> priorities,            
            CoroutineRunner coroutineRunner)
        {
            _lateInitializables = new List<LateInitializableInfo>();

            for (int i = 0; i < lateInitializables.Count; i++)
            {
                var lateInitializable = lateInitializables[i];

                // Note that we use zero for unspecified priority
                // This is nice because you can use negative or positive for before/after unspecified
                var matches = priorities.Where(x => lateInitializable.GetType().DerivesFromOrEqual(x.First)).Select(x => x.Second).ToList();
                int priority = matches.IsEmpty() ? 0 : matches.Distinct().Single();

                _lateInitializables.Add(new LateInitializableInfo(lateInitializable, priority));
            }
            
            this._coroutineRunner = coroutineRunner;
        }
        
        public void DelayInitialize()
        {
            _coroutineRunner.RunCoroutine(LateInitializeCoroutine());           
        }

        public void LateInitalize()
        {
            Assert.That(!_hasInitialized);
            _hasInitialized = true;

            _lateInitializables = _lateInitializables.OrderBy(x => x.Priority).ToList();
            
#if UNITY_EDITOR
            foreach (var lateInitializable in _lateInitializables.Select(x => x.LateInitializable).GetDuplicates())
            {
                Assert.That(false, "Found duplicate ILateInitializable with type '{0}'".Fmt(lateInitializable.GetType()));
            }
#endif            
            
            foreach (var lateInitializable in _lateInitializables)
            {
                try
                {
#if ZEN_INTERNAL_PROFILING
                    using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
#if UNITY_EDITOR
                    using (ProfileBlock.Start("{0}.Initialize()", lateInitializable.LateInitializable.GetType()))
#endif
                    {
                        lateInitializable.LateInitializable.LateInitialize();
                    }
                }
                catch (Exception e)
                {
                    throw Assert.CreateException(
                        e, "Error occurred while initializing IInitializable with type '{0}'", lateInitializable.LateInitializable.GetType());
                }
            }
        }

        private IEnumerator LateInitializeCoroutine()
        {
            yield return null;

            LateInitalize();
        }
        
        class LateInitializableInfo
        {
            public ILateInitializable LateInitializable;
            public int Priority;

            public LateInitializableInfo(ILateInitializable lateInitializable, int priority)
            {
                LateInitializable = lateInitializable;
                Priority = priority;
            }
        }        
    }
}