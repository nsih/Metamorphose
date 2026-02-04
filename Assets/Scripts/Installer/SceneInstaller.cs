using Reflex.Core;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using FMODUnity;
using TJR.Core.Interface;
using TJR.Core.GamePlay.Service;
using Reflex.Enums;

namespace TJR.Core.Installer
{
    public class SceneInstaller : MonoBehaviour, IInstaller
    {
        // 플레이어 초기화용 데이터
        [SerializeField] private PlayerStat _playerStatSO;
        [SerializeField] private PlayerWeaponData _startWeapon;

        [SerializeField] private PlayerSpawner _playerSpawner;
        [SerializeField] private MapUIManager _mapUIManager;
        [SerializeField] private MapManager _mapManager;
        [SerializeField] private EnemyPoolManager _enemyPoolManager;
        [SerializeField] private EventReference _musicEventReference;

        public void InstallBindings(ContainerBuilder builder)
        {
            builder.RegisterFactory((container) => new PlayerModel(_playerStatSO, _startWeapon), Lifetime.Scoped, Reflex.Enums.Resolution.Eager);

            builder.RegisterType(typeof(PlayerGoldService), Lifetime.Scoped, Reflex.Enums.Resolution.Eager);

            builder.RegisterValue(_playerSpawner);

            var roomProperty = new AsyncReactiveProperty<RoomManager>(null);
            builder.RegisterValue(
                roomProperty,
                new Type[] { typeof(AsyncReactiveProperty<RoomManager>), typeof(IReadOnlyAsyncReactiveProperty<RoomManager>) }
            );

            if (_mapUIManager != null)
                builder.RegisterValue(_mapUIManager);

            if (_mapManager != null)
                builder.RegisterValue(_mapManager);

            if (_enemyPoolManager != null)
                builder.RegisterValue(_enemyPoolManager);

            builder.OnContainerBuilt += OnBuilt;
        }

        void OnBuilt(Container container)
        {
            container.Single<IAudioService>().PlayMusic(_musicEventReference);
            container.Single<PlayerGoldService>().AddGold(100);
        }
    }
}

