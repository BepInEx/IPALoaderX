namespace IllusionPlugin
{
	public interface IEnhancedPlugin : IPlugin
	{
		/// <summary>
		///     Gets a list of executables this plugin should be excuted on (without the file ending)
		/// </summary>
		/// <example>{ "PlayClub", "PlayClubStudio" }</example>
		string[] Filter { get; }

		void OnLateUpdate();
	}
}