using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using photon.InGame.Entities.Actors.Heroes;
using photon.InGame.Entities.Chips;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;

namespace photon.InGame.Entities.Enemies;

public partial class EnemyFactory : Node
{
	[Export] private EnemyPack[] _enemyPacks;
	[Export] private StageArea _stageArea;
	[Export] private ChipFactory _chipFactory;
	private readonly List<IEnemy> _instances = new List<IEnemy>();
	private Hero _player;
	private SceneTree _tree;
	private CompositeDisposable _disposables;
	public double LowestFun { get; private set; }

	public override void _ExitTree()
	{
		base._ExitTree();
		_disposables?.Dispose();
	}

	public void Initialize(Hero player)
	{
		_disposables = new CompositeDisposable();
		_player = player;
		LowestFun = _enemyPacks.Min(x => x.Fun);
	}
	
	public double Create(double bore)
	{
		var spawnPosition = _stageArea.GetRandomSafetyPoint();
		
		// 敵を生成
		var enemy = GetRandomEnemy(bore);
		_instances.Add(enemy);
		enemy.OnDeath.Subscribe(_ => { }, () => _chipFactory.Create(enemy.Position, enemy.Fun)).AddTo(_disposables);
		enemy.OnRemove.Subscribe(_ => { }, () => _instances.Remove(enemy)).AddTo(_disposables);
		enemy.Initialize(spawnPosition, _player);
		return bore - enemy.Fun;
	}

	public void KillAll()
	{
		var tempList = new List<IEnemy>(_instances);
		foreach(var enemy in tempList)
		{
			enemy.Die();
		}
		GD.Print("EnemyFactory.KillAll: done");
	}
	
	/// <summary>
	/// ランダムなエネミーを返す。ただしfunがbore以下のエネミーのみ。
	/// </summary>
	/// <param name="bore"></param>
	/// <returns></returns>
	private IEnemy GetRandomEnemy(double bore)
	{
		var allowList = _enemyPacks.Where(x => x.Fun < bore).ToList();
		if (allowList.Count == 0) throw new Exception("Cannot find enemy with fun < bore.");
		var randomId = GD.RandRange(0, allowList.Count - 1);
		var pack = allowList[randomId];
		var instance = pack.Instantiate();
		AddChild(instance);
		return instance;
	}
	
	public double GetTotalFun()
	{
		return _instances.Sum(x => x.Fun);
	}
}
