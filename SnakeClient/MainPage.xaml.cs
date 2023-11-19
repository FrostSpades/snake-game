// Authors: Ethan Andrews and Mary Garfield
// Mainpage of the application
// University of Utah
namespace SnakeGame;
using GameController;
using Microsoft.UI.Xaml.Controls;

/// <summary>
/// Class for representing the main page of the maui application.
/// </summary>
public partial class MainPage : ContentPage
{
    GameController controller;

    /// <summary>
    /// Default constructor
    /// </summary>
    public MainPage()
    {
        controller = new GameController();

        // Register the controller events
        controller.GameUpdate += OnFrame;
        controller.Error += ConnectionError;
        controller.Success += ConnectionSuccess;
        controller.Closed += Exit;

        InitializeComponent();

        worldPanel.SetModel(controller.GetModel());

        graphicsView.Invalidate();
    }
    
    /// <summary>
    /// If there is a connection error, display an error.
    /// </summary>
    void ConnectionError()
    {
        DisplayAlert("Error", "Unable to connect to server", "Close");
    }

    /// <summary>
    /// If the connection was successful, disable the connection button
    /// </summary>
    void ConnectionSuccess()
    {
        Dispatcher.Dispatch(() => connectButton.IsEnabled = false);
    }

    /// <summary>
    /// If there was an error after the connection process, quit the app
    /// </summary>
    void Exit()
    {
        Dispatcher.Dispatch(() => App.Current.Quit());
    }

    /// <summary>
    /// Resets the keyboard focus.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void OnTapped(object sender, EventArgs args)
    {
        keyboardHack.Focus();
    }

    /// <summary>
    /// Sends controls to the controller on keyboard input.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void OnTextChanged(object sender, EventArgs args)
    {
        Entry entry = (Entry)sender;
        String text = entry.Text.ToLower();

        // Sends the text to the controller
        if (text == "w")
        {
            controller.Move("up");
        }
        else if (text == "a")
        {
            controller.Move("left");
        }
        else if (text == "s")
        {
            controller.Move("down");
        }
        else if (text == "d")
        {
            controller.Move("right");
        }
        entry.Text = "";
    }

    private void NetworkErrorHandler()
    {
        DisplayAlert("Error", "Disconnected from server", "OK");
    }


    /// <summary>
    /// Event handler for the connect button
    /// We will put the connection attempt interface here in the view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ConnectClick(object sender, EventArgs args)
    {
        if (serverText.Text == "")
        {
            DisplayAlert("Error", "Please enter a server address", "OK");
            return;
        }
        if (nameText.Text == "")
        {
            DisplayAlert("Error", "Please enter a name", "OK");
            return;
        }
        if (nameText.Text.Length > 16)
        {
            DisplayAlert("Error", "Name must be less than 16 characters", "OK");
            return;
        }
        controller.Connect(serverText.Text, nameText.Text);

        keyboardHack.Focus();
    }

    /// <summary>
    /// Use this method as an event handler for when the controller has updated the world
    /// </summary>
    public void OnFrame()
    {
        Dispatcher.Dispatch(() => graphicsView.Invalidate());
    }

    /// <summary>
    /// Displays the controls.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ControlsButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Controls",
                     "W:\t\t Move up\n" +
                     "A:\t\t Move left\n" +
                     "S:\t\t Move down\n" +
                     "D:\t\t Move right\n",
                     "OK");
    }

    /// <summary>
    /// Displays about dialogue.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AboutButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("About",
      "SnakeGame solution\nArtwork by Jolie Uk and Alex Smith\nGame design by Daniel Kopta and Travis Martin\n" +
      "Implementation by Ethan Andrews and Mary Garfield\n" +
        "CS 3500 Fall 2022, University of Utah", "OK");
    }

    /// <summary>
    /// Returns focus to the content page.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContentPage_Focused(object sender, FocusEventArgs e)
    {
        if (!connectButton.IsEnabled)
            keyboardHack.Focus();
    }
}