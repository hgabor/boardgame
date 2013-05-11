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
using System.Windows.Threading;

namespace Level14.BoardGameGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        enum SelectState {
            From,
            To,
            Special
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
                gamePanel.ClearHighlight();
                offBoard.ClearHighlight();
                if (value != null)
                {
                    offBoard.AddHighlight(value);
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
                if (value == null || Coords.Match(value, new Coords(-1)))
                {
                    fromCoord = null;
                    gamePanel.ClearHighlight();
                }
                else
                {
                    fromCoord = value;
                    gamePanel.AddHighlight(value);
                    foreach (var md in game.EnumerateMovesFromCoord(value))
                    {
                        gamePanel.AddHighlight(md.To);
                    }
                }
            }
        }
        Coords toCoord;

        IEnumerable<Piece> mustSelectFrom;

        public MainWindow()
        {
            InitializeComponent();

            offBoard.Selected += (sender, args) =>
            {
                switch (selectState)
                {
                    case SelectState.From:
                        FromOffboard = args.Piece;
                        selectState = SelectState.To;
                        break;
                    case SelectState.To:
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
                        break;
                    case SelectState.Special:
                        if (mustSelectFrom.Contains(args.Piece))
                        {
                            SetSelected(args.Piece);
                        }
                        break;
                }
                gamePanel.InvalidateVisual();
                offBoard.InvalidateVisual();
            };

            gamePanel.Selected += (sender, args) =>
            {
                switch (selectState)
                {
                    case SelectState.From:
                        FromCoord = args.Coords;
                        if (FromCoord != null)
                        {
                            selectState = SelectState.To;
                        }
                        break;
                    case SelectState.To:
                        if (fromCoord != null && Coords.Match(fromCoord, args.Coords))
                        {
                            FromCoord = null;
                            selectState = SelectState.From;
                        }
                        else
                        {
                            toCoord = args.Coords;
                            MakeMove();
                        }
                        break;
                    case SelectState.Special:
                        foreach (var kvp in game.GetPieces())
                        {
                            if (Coords.Match(kvp.Key, args.Coords) && mustSelectFrom.Contains(kvp.Value))
                            {
                                SetSelected(kvp.Value);
                                return;
                            }
                        }
                        break;
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

        string gamename;
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
                if (game.PlayerCount == 1)
                {
                    if (game.Winners.Count() == 1)
                    {
                        System.Windows.MessageBox.Show("You won!");
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("You lost!");
                    }
                }
                else
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
                }
                NewGame();
            }
            selectState = SelectState.From;
            FromCoord = null;
            FromOffboard = null;
            toCoord = null;
            currentPlayerLabel.Content = game.CurrentPlayer.ToString();
        }

        private void SetCoordTransformation(Game game)
        {
            if (game.Size.Dimension == 3)
            {
                // Suppose mills
                gamePanel.ICT = new CoordTransformer(
                    new Coords[] {
                        new Coords(1,1,1), new Coords(2,1,1), new Coords(3,1,1),
                        new Coords(1,2,1),                    new Coords(3,2,1),
                        new Coords(1,3,1), new Coords(2,3,1), new Coords(3,3,1),
                        new Coords(1,1,2), new Coords(2,1,2), new Coords(3,1,2),
                        new Coords(1,2,2),                    new Coords(3,2,2),
                        new Coords(1,3,2), new Coords(2,3,2), new Coords(3,3,2),
                        new Coords(1,1,3), new Coords(2,1,3), new Coords(3,1,3),
                        new Coords(1,2,3),                    new Coords(3,2,3),
                        new Coords(1,3,3), new Coords(2,3,3), new Coords(3,3,3),
                    },
                    new Coords[] {
                        new Coords(3,3), new Coords(5,3), new Coords(7,3),
                        new Coords(3,5),                  new Coords(7,5),
                        new Coords(3,7), new Coords(5,7), new Coords(7,7),
                        new Coords(2,2), new Coords(5,2), new Coords(8,2),
                        new Coords(2,5),                  new Coords(8,5),
                        new Coords(2,8), new Coords(5,8), new Coords(8,8),
                        new Coords(1,1), new Coords(5,1), new Coords(9,1),
                        new Coords(1,5),                  new Coords(9,5),
                        new Coords(1,9), new Coords(5,9), new Coords(9,9),
                    }
                    );
            }
            else
            {
                gamePanel.ICT = new IdentityTransformer();
            }
        }

        DispatcherFrame selectionFrame;
        Piece selectedPiece;
        private void SetSelected(Piece p)
        {
            selectedPiece = p;
            selectionFrame.Continue = false;
        }
        private Piece SelectPiece(IEnumerable<Piece> pieces)
        {
            selectState = SelectState.Special;
            this.mustSelectFrom = pieces;

            offBoard.ClearHighlight();
            gamePanel.ClearHighlight();
            foreach (var p in mustSelectFrom)
            {
                offBoard.AddHighlight(p);
                gamePanel.AddHighlight(p);
            }
            offBoard.InvalidateVisual();
            gamePanel.InvalidateVisual();

            selectionFrame = new DispatcherFrame();
            Dispatcher.PushFrame(selectionFrame);
            selectionFrame = null;

            selectState = SelectState.From;
            this.mustSelectFrom = null;

            return selectedPiece;
        }

        private void NewGame()
        {
            if (rulebook == null) return;
            this.game = new Game(rulebook);
            game.SetSelectPieceFunction(SelectPiece);

            var cache = new ImageCache(game, gamename);
            SetCoordTransformation(game);

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
                this.gamename = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                NewGame();
            }
        }

        private void newGameButton_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }
    }
}
