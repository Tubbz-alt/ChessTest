//
// Chess Game
//
// Copyright 2012 Jose Luis P. Cardenas
//
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace ChessTest
{
    class File
    {
        public void Save(ArrayList history, string result, GameType type)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PGN files (*.pgn)|*.pgn";
            Stream stream;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                if ((stream = saveDialog.OpenFile()) != null)
                {
                    StreamWriter sw = new StreamWriter(stream);
                    sw.Write(NotationToText(history, result, type));
                    sw.Close();
                    stream.Close();
                }
            }
        }

        public GameLoaded Load()
        {
            GameLoaded gload = new GameLoaded {};
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "PGN files (*.pgn)|*.pgn";
            Stream stream;

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                if ((stream = openDialog.OpenFile()) != null)
                {
                    StreamReader sr = new StreamReader(stream);
                    try
                    {
                        gload = TextToGame(sr.ReadToEnd());
                    } catch (Exception) {
                        MessageBox.Show("Error al cargar el juego", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    sr.Close();
                    stream.Close();
                }
            }

            return gload;
        }

        public string NotationToText(ArrayList history, string result, GameType type)
        {
            string output = "";
            output += "[Event \"MGChess Game\"]\r\n";
            output += "[Site \"Monterrey, México MEX\"]\r\n";
            output += "[Date \"????.??.??\"]\r\n";
            output += "[Round \"-\"]\r\n";
            output += "[White \"Human\"]\r\n";
            output += "[Black \"" + (type == GameType.HUMAN_HUMAN ? "Human" : "Computer") + "\"]\r\n";
            output += "[Result \"" + result + "\"]\r\n\r\n";

            for (int i = 0; i < history.Count; i++)
            {
                HMove move = (HMove)history[i];
                output += ((i%2) == 0 ? ((i/2)+1) + ".": "") + move.An + " ";
            }
            output += (result != "*" ? result : "");

            return output;
        }

        public GameLoaded TextToGame(string txt)
        {
            Hashtable tags = new Hashtable();
            ArrayList history = new ArrayList();
            string[] lines = txt.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (line.StartsWith("["))
                {
                    string tag = line.Substring(line.IndexOf('[') + 1, line.IndexOf(' ')-1);
                    int pos = line.IndexOf('"') + 1;
                    string value = line.Substring(pos, line.LastIndexOf('"')-pos);
                    tags.Add(tag.ToLower(), value);
                }
                else
                {
                    string[] moves = line.Split(new char[] { '.' });
                    for (int i = 1; i < moves.Length; i++)
                    {
                        string[] mov = moves[i].Trim().Split(new char[] { ' ' });
                        if (mov.Length > 1 || mov.Length == 1 && i == moves.Length - 1)
                        {
                            history.Add(new HMove { An = mov[0], Nn = "" });
                            if (mov.Length > 1)
                                history.Add(new HMove { An = mov[1], Nn = "" });
                        }
                        else
                        {
                            throw new Exception("Invalid PGN File");
                        }
                    }
                }
            }

            return new GameLoaded { Tags = tags, History = history };
        }

        public struct GameLoaded
        {
            public Hashtable Tags;
            public ArrayList History;
        }
    }
}
