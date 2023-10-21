using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MouseKnightGD.Entities.Actors.Heroes;

namespace MouseKnightGD.Entities.Enemies;

public abstract partial class EnemyBase : RigidBody2D, IEnemy
{
	[Export] private double _fun;
	[Export] private int _maxHp = 1;
	private int _hp;
	private readonly Subject<Unit> _onDead = new Subject<Unit>();
	private readonly Subject<Unit> _onRemove = new Subject<Unit>();
	protected Hero Player;
	public bool IsDead => _hp <= 0;
	public IObservable<Unit> OnDead => _onDead;
	public IObservable<Unit> OnRemove => _onRemove;
	public double Fun => _fun;

	public virtual void Initialize(Vector2 spawnPosition, Hero player)
	{
		_hp = _maxHp;
		Player = player;
		Position = spawnPosition;
		Visible = true;
	}

	public void Remove()
	{
		_onRemove.OnCompleted();
		QueueFree();
	}

	public void TakeDamage(int amount)
	{
		if (IsDead) return;
		_hp -= amount;
		if (IsDead) Die();
	}
	
	public void Die()
	{
		_onDead.OnCompleted();
		Remove();
	}

	private void GiveDamage(Node body)
	{
		if (body is Hero hero)
		{
			hero.TakeDamage(1);
		}
	}
}
