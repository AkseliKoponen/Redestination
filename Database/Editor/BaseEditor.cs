namespace RD.DB
{
	public abstract class BaseEditor : UnityEditor.Editor
	{
		public void OnEnable()
		{
			Dirtify();
		}

		protected abstract void Dirtify();
	}
}
