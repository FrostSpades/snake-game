// Authors: Ethan Andrews and Mary Garfield
// Creation of application
// University of Utah

namespace SnakeGame;

/// <summary>
/// Application creation.
/// </summary>
public partial class App : Application
{
	/// <summary>
	/// Default constructor
	/// </summary>
	public App()
	{
		InitializeComponent();

		MainPage = new MainPage();
    }
}

