using GameTemplate.Scripts.Systems.Audio;
using GameTemplate.Scripts.Systems.Input;
using GameTemplate.Scripts.Systems.Loading;
using GameTemplate.Scripts.Systems.Scene;
using GameTemplate.Systems.Audio;
using GameTemplate.Systems.Pooling;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameTemplate.Scripts.Core.Scopes
{
    /// <summary>
    /// An entry point to the application, where we bind all the common dependencies to the root DI scope.
    /// </summary>
    public class ApplicationScope : LifetimeScope
    {
        public AudioDataSO audioDataSo;
        public SceneDataSO sceneDataSo;
        public PoolingDataSO poolingDataSo;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterInstance(audioDataSo);
            builder.RegisterInstance(sceneDataSo);
            builder.RegisterInstance(poolingDataSo);

            builder.Register<AudioService>(Lifetime.Singleton);
            builder.Register<PoolingService>(Lifetime.Singleton);
            builder.Register<ISceneService, SceneService>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<LoadingScreenController>();
            
            // Register the generated Controls class as a singleton
            builder.Register<Controls>(Lifetime.Singleton).AsSelf();
        }

        protected override async void Awake()
        {
            base.Awake();
            
            // Ensure SoundService is ready before Main Menu
            var audioService = Container.Resolve<AudioService>();
            await audioService.InitializeAsync();
            
            var sceneService = Container.Resolve<ISceneService>();
            sceneService.LoadMenuScene();
        }

        public void Start()
        {
            Application.targetFrameRate = 60;
            //SceneManager.LoadScene("MainMenu");
        }
    }
}