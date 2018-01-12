using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Konik_Forms
{
    public partial class Form1 : Form
    {

        int boardSize = 1;
        int _boardX = 10, _boardY = 10;
        int _boardWidth = 240;
        int _boardHeight = 240;
        int visualizationSpeed = 1;
        long numberOfInterations = 0;
        Brush _borderColor = Brushes.Black;
        Brush _startColor = Brushes.Green;
        Brush _endColor = Brushes.Red;
        Brush _visitedField = Brushes.Gold;
        Brush _knightColor = Brushes.Orange;
        Brush[] _boardColor = { Brushes.White, Brushes.Black };

        board chessboard;
        List<knightMove> knightMoves = new List<knightMove>();

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) //Size
        {
           
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        
        private void StartButton_Click(object sender, EventArgs e)
        {
            

            if (textBox1.Text != "") {
                boardSize = Int32.Parse(textBox1.Text);
            };

            boardPoint startPoint = new boardPoint(Int32.Parse(textBox2.Text)-1, Int32.Parse(textBox4.Text)-1);
            boardPoint endPoint = new boardPoint(Int32.Parse(textBox3.Text)-1, Int32.Parse(textBox5.Text)-1);

            
            chessboard = new board(boardSize, startPoint, endPoint);
            chessboard.startPoint = startPoint;
            chessboard.endPoint = endPoint;

            this.Paint += Form1_Paint;
            this.Invalidate();

        }


        void drawChessBoard(Graphics g)
        {
            //draw squares
            int boardSize = chessboard.boardSize;

            int spacingX = _boardWidth / boardSize;
            int spacingY = _boardHeight / boardSize;
            for (int col = 0; col < boardSize; col++)
            {
                for (int row = 0; row < boardSize; row++)
                {
                
                    g.FillRectangle(_boardColor[(col + row) % 2], _boardX + col * spacingX, _boardY + row * spacingY, spacingX, spacingY);
                }
            }

            //draw visited points
            var visitedPoints = chessboard.getVisitedPoints();
            foreach (var point in visitedPoints)
            {
                g.FillRectangle(_visitedField, _boardX + point.coordinateX * spacingX, _boardY + point.coordinateY * spacingY, spacingX, spacingY);
            }

            //draw start and end points
            g.FillRectangle(_endColor, _boardX + chessboard.endPoint.coordinateX * spacingX, _boardY + chessboard.endPoint.coordinateY * spacingY, spacingX, spacingY);
            g.FillRectangle(_startColor, _boardX + chessboard.startPoint.coordinateX * spacingX, _boardY + chessboard.startPoint.coordinateY * spacingY, spacingX, spacingY);

            //draw current knight position
            g.FillRectangle(_knightColor, _boardX + chessboard.getKnightPoint().coordinateX * spacingX, _boardY + chessboard.getKnightPoint().coordinateY * spacingY, spacingX, spacingY);


            //draw border
            g.DrawRectangle(new Pen(_borderColor, 1), new Rectangle(_boardX, _boardY, _boardWidth, _boardHeight));
        }

        private void MoveKnight_Click(object sender, EventArgs e)
        {
            //create the first postion with no move
            
            knightMoves.Clear();
            knightMoves.Add(new knightMove(1));
            knightMoves.LastOrDefault().currentPoint = chessboard.startPoint;

            Status.Text = "Calculating...";
            this.Invalidate();
            Application.DoEvents();

            while (!chessboard.checkSuccessCondition())
            {
                numberOfInterations += 1;
                label7.Text = "Number of Interations: " + numberOfInterations;
                label8.Text = "Step number: " + knightMoves.Count();
                //set wrong move to ignore
                chessboard.wrongMoves = knightMoves.ElementAt(knightMoves.Count - 1).wrongMoves;
                if (chessboard.getAvilablePoints().Count() > 0)
                {
                    //add new move
                    knightMoves.Add(new knightMove(knightMoves.Max(p => p.moveId) + 1));
                    //if any moves exists add current position as previous position 
                    knightMoves.LastOrDefault().previousPoint = chessboard.getKnightPoint();

                    //really move the knight 
                    
                    chessboard.moveKnight();

                    //get new position as current position
                    knightMoves.LastOrDefault().currentPoint = chessboard.getKnightPoint();
                }
                else
                {

                    //save current point
                    var currentPoint = chessboard.getKnightPoint();

                    //add current point to the wrong moves list
                    knightMoves.LastOrDefault().wrongMoves.Add(currentPoint);

                    //move the knight back
                    chessboard.undoKnight(knightMoves.LastOrDefault().previousPoint , knightMoves.LastOrDefault().currentPoint);

                    //remove the last (wrong) move from the list
                    knightMoves.RemoveAt(knightMoves.Count - 1);

                    //add current point to the wrong moves list
                    knightMoves.LastOrDefault().wrongMoves.Add(currentPoint);


                }
                if(checkBox1.Checked == true)
                {
                    visualizationSpeed = trackBar1.Value;
                    System.Threading.Thread.Sleep(1000 / visualizationSpeed);
                    this.Invalidate();
                    Application.DoEvents();
                }
                                    
                
            }
            this.Invalidate();

            Status.Text = "Please enter data";
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            visualizationSpeed = trackBar1.Value;
        }

        //Form1 Paint method calls DrawChessBoard


        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            drawChessBoard(e.Graphics);
        }
    }
    public class boardPoint
    {
        public int coordinateX { get; }
        public int coordinateY { get; }
        public bool visited { get; set; }
        public bool knightVisiting { get; set; }

        public boardPoint(int x, int y)
        {
            this.coordinateX = x;
            this.coordinateY = y;
        }

    }

    public class board
    {
        private int _boardSize = 1;
        public int boardSize
        {
            get
            {
                return _boardSize;
            }
            set
            {
                if (boardSize <= 0)
                {
                    throw new Exception("Rozmiar tablicy musi być większy od 0!");
                }
                else
                {
                    _boardSize = value;
                }
            }
        }

        private boardPoint _startPoint;
        public boardPoint startPoint
        {
            get
            {
                return _startPoint;
            }
            set
            {
                if (value.coordinateX > boardSize || value.coordinateY > boardSize)
                {
                    throw new Exception("Punkt początkowy musi znajdować się na tablicy!");
                }
                else
                {
                    _startPoint = value;
                }
            }
        }

        private boardPoint _endPoint;
        public boardPoint endPoint
        {
            get
            {
                return _endPoint;
            }
            set
            {
                if (value.coordinateX > boardSize || value.coordinateY > boardSize)
                {
                    throw new Exception("Punkt końcowy musi znajdować się na tablicy!");
                }
                else
                {
                    _endPoint = value;
                }
            }
        }

        public List<boardPoint> wrongMoves { get; set; }

        private List<boardPoint> boardArray = new List<boardPoint>();

        
        public board(int boardSize, boardPoint startPoint, boardPoint endPoint)
        {
            this.boardSize = boardSize;
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (startPoint.coordinateX == i && startPoint.coordinateY == j)
                    {
                        startPoint.knightVisiting = true;
                        boardArray.Add(startPoint);

                    }
                    else if (endPoint.coordinateX == i && endPoint.coordinateY == j)
                    {
                        boardArray.Add(endPoint);
                    }
                    else
                    {
                        boardArray.Add(new boardPoint(i, j));
                    }
                }
            }
        }

        public List<boardPoint> getVisitablePoints()
        {
            return boardArray.Where(p =>       (p.coordinateX - 2 == getKnightPoint().coordinateX && p.coordinateY - 1 == getKnightPoint().coordinateY) ||
                                               (p.coordinateX - 2 == getKnightPoint().coordinateX && p.coordinateY + 1 == getKnightPoint().coordinateY) ||
                                               (p.coordinateX + 2 == getKnightPoint().coordinateX && p.coordinateY - 1 == getKnightPoint().coordinateY) ||
                                               (p.coordinateX + 2 == getKnightPoint().coordinateX && p.coordinateY + 1 == getKnightPoint().coordinateY) ||
                                               (p.coordinateX - 1 == getKnightPoint().coordinateX && p.coordinateY - 2 == getKnightPoint().coordinateY) ||
                                               (p.coordinateX - 1 == getKnightPoint().coordinateX && p.coordinateY + 2 == getKnightPoint().coordinateY) ||
                                               (p.coordinateX + 1 == getKnightPoint().coordinateX && p.coordinateY - 2 == getKnightPoint().coordinateY) ||
                                               (p.coordinateX + 1 == getKnightPoint().coordinateX && p.coordinateY + 2 == getKnightPoint().coordinateY)).ToList();
        }

        public List<boardPoint> getAvilablePoints()
        {
            return getVisitablePoints().Where(p => p.visited == false && 
                                                  !(p.coordinateX == endPoint.coordinateX && p.coordinateY == endPoint.coordinateY)).ToList().Except(wrongMoves).ToList();
        }

        public bool checkSuccessCondition()
        {
            int numberOfVisitedField = boardArray.Where(p => p.visited == true).Count();
            int totalNumberOfFields = Convert.ToInt32(Math.Pow(boardSize, 2));

            if (getVisitablePoints().Contains(endPoint))
            {
                if(startPoint == endPoint && numberOfVisitedField >= ( totalNumberOfFields - 1))
                {
                    return true;
                }
                else if(startPoint != endPoint && numberOfVisitedField >= (totalNumberOfFields - 2))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
            
        }

        public void moveKnight()
        {
            //randomly selects new jump for the knight
            Random rnd = new Random();


            var avilablePoints = getAvilablePoints();
            var knightPoint = getKnightPoint();
            int nextJump = rnd.Next(avilablePoints.Count());

            //moves the knight
            knightPoint.visited = true;
            knightPoint.knightVisiting = false;
            avilablePoints.ElementAt(nextJump).knightVisiting = true;
            
        }

        public void undoKnight(boardPoint previousPoint, boardPoint currentPoint)
        {
            currentPoint.knightVisiting = false;
            previousPoint.visited = false;
            previousPoint.knightVisiting = true;

        }

        public boardPoint getKnightPoint()
        {
            return boardArray.Where(p => p.knightVisiting == true).FirstOrDefault();
        }

        public List<boardPoint> getVisitedPoints()
        {
            return boardArray.Where(p => p.visited == true).ToList();
        }


    }

    public class knightMove
    {
        public int moveId { get; set; }
        public boardPoint previousPoint { get; set; }
        public boardPoint currentPoint { get; set; }
        public List<boardPoint> wrongMoves = new List<boardPoint>();

        public knightMove(int moveId)
        {
            this.moveId = moveId;
        }


    }
}
