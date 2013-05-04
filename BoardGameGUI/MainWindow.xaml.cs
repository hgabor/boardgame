using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Level14.BoardGameRules;

namespace Level14.BoardGameGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        enum SelectState {
            From,
            To
        }

        SelectState selectState = SelectState.From;
        Piece fromOffboard;
        Piece FromOffboard
        {
            get
            {
                return fromOffboard;
            }
            set
            {
                fromOffboard = value;
                offBoard.Highlighted = value;
                if (value == null)
                {
                    gamePanel.ClearHighlight();
                }
                else
                {
                    foreach (var md in game.EnumerateMovesFromOffboard(value))
                    {
                        gamePanel.AddHighlight(md.To);
                    }
                }
            }
        }
        Coords fromCoord;
        Coords FromCoord
        {
            get
            {
                return fromCoord;
            }
            set
            {
                fromCoord = value;
                if (value == null)
                {
                    gamePanel.ClearHighlight();
                }
                else
                {
                    gamePanel.AddHighlight(value);
                    foreach (var md in game.EnumerateMovesFromCoord(value))
                    {
                        gamePanel.AddHighlight(md.To);
                    }
                }
            }
        }
        Coords toCoord;

        public MainWindow()
        {
            InitializeComponent();

            offBoard.Selected += (sender, args) =>
            {
                if (selectState == SelectState.From)
                {
                    FromOffboard = args.Piece;
                    selectState = SelectState.To;
                }
                else
                {
                    if (FromOffboard == args.Piece)
                    {
                        // Unselect it
                        FromOffboard = null;
                        selectState = SelectState.From;
                    }
                    else if (FromOffboard != null)
                    {
                        FromOffboard = args.Piece;
                    }
                    else
                    {
                        // Unsupported
                    }
                }
                gamePanel.InvalidateVisual();
                offBoard.InvalidateVisual();
            };

            gamePanel.Selected += (sender, args) =>
            {
                if (selectState == SelectState.From)
                {
                    FromCoord = args.Coords;
                    selectState = SelectState.To;
                }
                else
                {
                    if (fromCoord != null && Coords.Match(fromCoord, args.Coords))
                    {
                        FromCoord = null;
                        selectState = SelectState.From;
                    }
                    else {
                        toCoord = args.Coords;
                        MakeMove();
                    }
                }
                gamePanel.InvalidateVisual();
                offBoard.InvalidateVisual();
            };

            App.Current.DispatcherUnhandledException += (sender, args) =>
            {
                InvalidGameException ex = args.Exception as InvalidGameException;
                if (ex != null)
                {
                    System.Windows.MessageBox.Show(ex.Message, "Invalid game", MessageBoxButton.OK, MessageBoxImage.Error);
                    args.Handled = true;
                }
            };
        }

        string rulebook;
        Game game;

        private void MakeMove()
        {
            if (fromOffboard != null && toCoord != null)
            {
                // From offboard to board
                game.TryMakeMoveFromOffboard(FromOffboard, toCoord);

                offBoard.Refresh();
                gamePanel.InvalidateVisual();
            }
            else if (fromCoord != null && toCoord != null)
            {
                // From coords to coords
                game.TryMakeMove(FromCoord, toCoord);

                offBoard.Refresh();
                gamePanel.InvalidateVisual();
            }

            if (game.GameOver)
            {
                if (game.Winners.Count() == 0)
                {
                    System.Windows.MessageBox.Show("Game over!\nThe game ended in a tie!");
                }
                else if (game.Winners.Count() == 1)
                {
                    System.Windows.MessageBox.Show("Game over!\nThe winner is: " + game.Winners.First().ToString());
                }
                else
                {
                    System.Windows.MessageBox.Show("Game over!\nThe winners are: " + string.Join(", ", game.Winners));
                }
                NewGame();
            }
            selectState = SelectState.From;
            FromCoord = null;
            FromOffboard = null;
            toCoord = null;
            currentPlayerLabel.Content = game.CurrentPlayer.ToString();
        }

        private void NewGame()
        {
            if (rulebook == null) return;
            this.game = new Game(rulebook);
            var cache = new ImageCache(game);
            gamePanel.Game = game;
            gamePanel.ImageCache = cache;

            offBoard.Game = game;
            offBoard.ImageCache = cache;

            gamePanel.InvalidateVisual();

            offBoard.Refresh();

            currentPlayerLabel.Content = game.CurrentPlayer.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            

            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".game";
            dlg.Filter = "Rulebooks (*.game)|*.game|All Files|*.*";
            bool? result = dlg.ShowDialog(this);

            if (result == true)
            {
                string fileName = dlg.FileName;
                this.rulebook = System.IO.File.ReadAllText(fileName);
                NewGame();
            }
        }

        private void newGameButton_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }
    }
}
