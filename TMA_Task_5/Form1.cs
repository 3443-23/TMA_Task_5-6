using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TMA_Task_5
{
    public partial class Form1 : Form
    {
        private int[,] board = new int[9, 9];
        private TextBox[,] textBoxes = new TextBox[9, 9];
        private Timer autoSaveTimer;

        public Form1()
        {
            InitializeComponent();
            InitializeGrid();

            // Автосохранение каждую минуту
            autoSaveTimer = new Timer();
            autoSaveTimer.Interval = 60000; // 60 секунд
            autoSaveTimer.Tick += AutoSaveTimer_Tick;
            autoSaveTimer.Start();

            // Загружаем сохранённое состояние, если оно есть
            if (File.Exists("sudoku_state.txt"))
            {
                LoadGameState("sudoku_state.txt");
                RefreshGrid();
            }
        }

        private void InitializeGrid()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    textBoxes[i, j] = new TextBox();
                    textBoxes[i, j].Width = 40;
                    textBoxes[i, j].Height = 40;
                    textBoxes[i, j].MaxLength = 1;
                    textBoxes[i, j].TextAlign = HorizontalAlignment.Center;
                    textBoxes[i, j].Font = new Font("Arial", 14);
                    textBoxes[i, j].Location = new Point(50 + j * 45, 50 + i * 45);
                    this.Controls.Add(textBoxes[i, j]);
                    textBoxes[i, j].TextChanged += new EventHandler(TextBox_TextChanged);
                }
            }
        }

        private void difficultyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedDifficulty = difficultyComboBox.SelectedItem.ToString();
            LoadSudokuForDifficulty(selectedDifficulty);
        }

        private void LoadSudokuForDifficulty(string difficulty)
        {
            string fileName = string.Empty;

            switch (difficulty)
            {
                case "Легкий":
                    fileName = "easy_sudoku.txt";
                    break;
                case "Средний":
                    fileName = "medium_sudoku.txt";
                    break;
                case "Сложный":
                    fileName = "hard_sudoku.txt";
                    break;
                default:
                    MessageBox.Show("Выберите уровень сложности");
                    return;
            }

            LoadGameState(fileName);
            RefreshGrid(); // После загрузки обновляем поле
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int row = (tb.Location.Y - 50) / 45;
            int col = (tb.Location.X - 50) / 45;

            if (tb.ReadOnly)
                return;

            if (int.TryParse(tb.Text, out int value))
            {
                board[row, col] = value;
            }
            else
            {
                board[row, col] = 0;
            }

            UpdateCellColors(row, col);
        }

        private void UpdateCellColors(int row, int col)
        {
            bool isValid = IsValid(board);

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    bool isInRow = (i == row);
                    bool isInCol = (j == col);
                    bool isInBox = (i / 3 == row / 3 && j / 3 == col / 3);

                    if (isInRow || isInCol || isInBox)
                    {
                        textBoxes[i, j].BackColor = isValid ? Color.LightGreen : Color.Red;
                    }
                    else
                    {
                        textBoxes[i, j].BackColor = Color.White;
                    }
                }
            }
        }

        private bool IsValid(int[,] board)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (board[row, col] == 0)
                        continue;

                    for (int j = 0; j < 9; j++)
                        if (j != col && board[row, col] == board[row, j])
                            return false;

                    for (int i = 0; i < 9; i++)
                        if (i != row && board[row, col] == board[i, col])
                            return false;

                    int startRow = (row / 3) * 3;
                    int startCol = (col / 3) * 3;
                    for (int i = startRow; i < startRow + 3; i++)
                        for (int j = startCol; j < startCol + 3; j++)
                            if ((i != row || j != col) && board[row, col] == board[i, j])
                                return false;
                }
            }
            return true;
        }

        private void solveButton_Click(object sender, EventArgs e)
        {
            SolveSudoku(board);
            RefreshGrid();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveGameState();
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            LoadGameState("easy_sudoku.txt");
            RefreshGrid();
        }

        private void SaveGameState()
        {
            using (StreamWriter writer = new StreamWriter("sudoku_state.txt"))
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        writer.Write(board[i, j] + " ");
                    }
                    writer.WriteLine();
                }
            }
        }

        private void LoadGameState(string fileName)
        {
            try
            {
                using (StreamReader reader = new StreamReader(fileName))
                {
                    for (int i = 0; i < 9; i++)
                    {
                        string line = reader.ReadLine();
                        string[] cells = line.Split(' ');
                        for (int j = 0; j < 9; j++)
                        {
                            board[i, j] = int.Parse(cells[j]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке: " + ex.Message);
            }
        }

        private bool SolveSudoku(int[,] board)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (board[row, col] == 0)
                    {
                        for (int num = 1; num <= 9; num++)
                        {
                            if (IsSafe(board, row, col, num))
                            {
                                board[row, col] = num;
                                if (SolveSudoku(board))
                                    return true;
                                board[row, col] = 0;
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        private bool IsSafe(int[,] board, int row, int col, int num)
        {
            return IsRowSafe(board, row, num) &&
                   IsColSafe(board, col, num) &&
                   IsBoxSafe(board, row, col, num);
        }

        private bool IsRowSafe(int[,] board, int row, int num)
        {
            for (int col = 0; col < 9; col++)
                if (board[row, col] == num)
                    return false;
            return true;
        }

        private bool IsColSafe(int[,] board, int col, int num)
        {
            for (int row = 0; row < 9; row++)
                if (board[row, col] == num)
                    return false;
            return true;
        }

        private bool IsBoxSafe(int[,] board, int row, int col, int num)
        {
            int startRow = row - row % 3;
            int startCol = col - col % 3;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[startRow + i, startCol + j] == num)
                        return false;
            return true;
        }

        private void RefreshGrid()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    textBoxes[i, j].Text = board[i, j] == 0 ? "" : board[i, j].ToString();

                    if (board[i, j] != 0)
                    {
                        textBoxes[i, j].ReadOnly = true;
                        textBoxes[i, j].BackColor = Color.LightGreen;
                    }
                    else
                    {
                        textBoxes[i, j].ReadOnly = false;
                        textBoxes[i, j].BackColor = Color.White;
                    }
                }
            }
        }

        // Автосохранение по таймеру
        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            SaveGameState();
        }
    }
}
