/* UserInterface.cs
 * Author: Rod Howell
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Ksu.Cis300.TrieLibrary;

namespace Ksu.Cis300.Boggle
{
    /// <summary>
    /// A user interface for a program that finds all words in a randomly-generated Boggle Deluxe board.
    /// </summary>
    public partial class UserInterface : Form
    {
        /// <summary>
        /// The length and width of the grid.
        /// </summary>
        private const int _gridSize = 5;

        /// <summary>
        /// The minimum allowed length of a word.
        /// </summary>
        private const int _minimumWordLength = 4;

        /// <summary>
        /// The current board contents.
        /// </summary>
        private string[,] _board = new string[_gridSize, _gridSize];

        /// <summary>
        /// The dice.
        /// </summary>
        private string[][] _dice = new string[][]
        {
            new string[] { "A", "F", "I", "R", "S", "Y" },
            new string[] { "A", "D", "E", "N", "N", "N" },
            new string[] { "A", "E", "E", "E", "E", "M" },
            new string[] { "A", "A", "A", "F", "R", "S" },
            new string[] { "A", "E", "G", "M", "N", "N" },
            new string[] { "A", "A", "E", "E", "E", "E" },
            new string[] { "A", "E", "E", "G", "M", "U" },
            new string[] { "A", "A", "F", "I", "R", "S" },
            new string[] { "B", "J", "K", "Qu", "X", "Z" },
            new string[] { "C", "C", "E", "N", "S", "T" },
            new string[] { "C", "E", "I", "L", "P", "T" },
            new string[] { "C", "E", "I", "I", "L", "T" },
            new string[] { "C", "E", "I", "P", "S", "T" },
            new string[] { "D", "H", "L", "N", "O", "R" },
            new string[] { "D", "H", "L", "N", "O", "R" },
            new string[] { "D", "D", "H", "N", "O", "T" },
            new string[] { "D", "H", "H", "L", "O", "R" },
            new string[] { "E", "N", "S", "S", "S", "U" },
            new string[] { "E", "M", "O", "T", "T", "T" },
            new string[] { "E", "I", "I", "I", "T", "T" },
            new string[] { "F", "I", "P", "R", "S", "Y" },
            new string[] { "G", "O", "R", "R", "V", "W" },
            new string[] { "I", "P", "R", "R", "R", "Y" },
            new string[] { "N", "O", "O", "T", "U", "W" },
            new string[] { "O", "O", "O", "T", "T", "U" }
        };

        /// <summary>
        /// The random number generator.
        /// </summary>
        private Random _randomNumbers = new Random();

        /// <summary>
        /// The list of allowable words.
        /// </summary>
        private ITrie _wordList = new TrieWithNoChildren();

        /// <summary>
        /// Constructs the GUI.
        /// </summary>
        public UserInterface()
        {
            InitializeComponent();
            GenerateNewBoard();
        }

        /// <summary>
        /// Generates a new game board.
        /// </summary>
        private void GenerateNewBoard()
        {
            int k = _dice.Length;
            for (int i = 0; i < _gridSize; i++)
            {
                FlowLayoutPanel row = (FlowLayoutPanel)uxBoard.Controls[i];
                for (int j = 0; j < _gridSize; j++)
                {
                    int loc = _randomNumbers.Next(k);
                    k--;
                    string[] temp = _dice[loc];
                    _dice[loc] = _dice[k];
                    _dice[k] = temp;
                    row.Controls[j].Text = _dice[k][_randomNumbers.Next(6)];
                    _board[i, j] = row.Controls[j].Text.ToString().ToLower();
                }
            }
        }

        /// <summary>
        /// Handles a Load event on the GUI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserInterface_Load(object sender, EventArgs e)
        {
            uxOpenDialog.ShowDialog();
            try
            {
                using (StreamReader input = File.OpenText(uxOpenDialog.FileName))
                {
                    while (!input.EndOfStream)
                    {
                        string word = input.ReadLine();
                        if (word.Length >= _minimumWordLength)
                        {
                            _wordList = _wordList.Add(word);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Application.Exit();
            }
        }

        /// <summary>
        /// Handles a Click event on the New Board button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uxNewBoard_Click(object sender, EventArgs e)
        {
            GenerateNewBoard();
        }

        /// <summary>
        /// Gets all words in the given results, plus all words formed by starting 
        /// with the content of the given StringBuilder, followed by a word that is 
        /// both contained in the given trie of completions and formed by the letters
        /// on a nonempty path that starts from the given board location, does not
        /// use any die indicated as used in the given array, and does not use any
        /// die more than once.
        /// </summary>
        /// <param name="row">The row containing the current die.</param>
        /// <param name="col">The column containing the current die.</param>
        /// <param name="used">Element [i, j] indicates that the die at location
        /// [i, j] has been used in the path to the current die.</param>
        /// <param name="path">The letters on the path to the current die.</param>
        /// <param name="completions">All completions of the letters on the path to
        /// form valid words.</param>
        /// <param name="results">The words found so far.</param>
        /// <returns>All the words found.</returns>
        private ITrie GetWords(int row, int col, bool[,] used, StringBuilder path, ITrie completions, ITrie results)
        {
            completions = completions.GetCompletions(_board[row, col]);
            if (completions == null)
            {
                return results;
            }
            else
            {
                used[row, col] = true;
                path.Append(_board[row, col]);
                if (completions.Contains(""))
                {
                    results = results.Add(path.ToString());
                }
                for (int i = Math.Max(0, row - 1); i < Math.Min(_gridSize, row + 2); i++)
                {
                    for (int j = Math.Max(0, col - 1); j < Math.Min(_gridSize, col + 2); j++)
                    {
                        if (!used[i, j])
                        {
                            results = GetWords(i, j, used, path, completions, results);
                        }
                    }
                }
                used[row, col] = false;
                path.Length -= _board[row, col].Length;
                return results;
            }
        }

        /// <summary>
        /// Handles a Click event on the "Find Words" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uxFindWords_Click(object sender, EventArgs e)
        {
            ITrie results = new TrieWithNoChildren();
            bool[,] used = new bool[_gridSize, _gridSize];
            StringBuilder prefix = new StringBuilder();

            for (int i = 0; i < _gridSize; i++)
            {
                for (int j = 0; j < _gridSize; j++)
                {
                    results = GetWords(i, j, used, prefix, _wordList, results);
                }
            }

            uxWordsFound.Items.Clear();
            uxWordsFound.BeginUpdate();
            results.AddAll(new StringBuilder(), uxWordsFound.Items);
            uxWordsFound.EndUpdate();
        }
    }
}
