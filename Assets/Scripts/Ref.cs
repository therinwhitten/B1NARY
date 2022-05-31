using System;

public class Ref<T>
{
	private readonly Func<T> getter;
	private readonly Action<T> setter;
	public Ref(Func<T> getter, Action<T> setter)
	{
		this.setter = setter;
		this.getter = getter;
	}
	public T Value { get => getter(); set => setter(value); }
}