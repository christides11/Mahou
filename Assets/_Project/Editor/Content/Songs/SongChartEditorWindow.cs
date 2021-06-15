using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Mahou.Content
{
    public class SongChartEditorWindow : EditorWindow
    {
        public enum NoteSnapType
        {
            NONE = 0,
            WHOLE = 1,
            HALF = 2,
            QUARTER = 3,
            EIGHTH = 4,
            SIXTEENTH = 5,
            THIRTHY_SECOND = 6
        }
        public float[] snapMovement = new float[7];
        public GameObject playObject;

        public SongChart songChart;
        public SongPiece songPiece;

        public NoteSnapType snapType = NoteSnapType.NONE;

        float currMove = 0;
        float playRate = 1;

        // TEXTURE
        int textureHeight = 0;
        int samplesPerPixel = 1;
        Texture2D[] waveFormTextures = null;
        
        float relativeTexturePositon = 0;
        float samplesModifier = 0.002f;
        float lineSamplesModifier = 0.004f;

        public static void Init(SongChart songChart)
        {
            SongChartEditorWindow window = ScriptableObject.CreateInstance<SongChartEditorWindow>();
            window.songChart = songChart;
            window.Show();
        }

        private void Update()
        {
            Repaint();
        }

        Vector2 scrollView;
        protected virtual void OnGUI()
        {
            if(songChart == null)
            {
                Close();
                return;
            }

            songPiece = (SongPiece)EditorGUILayout.ObjectField("Piece", songPiece, typeof(SongPiece), false);
            if (songPiece == null)
            {
                return;
            }
            if(playObject == null)
            {
                playObject = new GameObject();
                playObject.hideFlags = HideFlags.HideAndDontSave;
                playObject.AddComponent<AudioSource>();
                playObject.GetComponent<AudioSource>().clip = songPiece.song;
            }
            playObject.GetComponent<AudioSource>().pitch = playRate;

            if (waveFormTextures == null)
            {
                GenerateWaveFormTexture();
                relativeTexturePositon = 0;
                CalculateMoveNumbers();
                return;
            }

            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    {
                        if (Event.current.keyCode == (KeyCode.LeftArrow))
                        {
                            if((int)snapType - 1 < 0)
                            {
                                snapType = NoteSnapType.THIRTHY_SECOND;
                                break;
                            }
                            snapType--;
                        }
                        if(Event.current.keyCode == KeyCode.RightArrow)
                        {
                            if((int)snapType + 1 > 6)
                            {
                                snapType = NoteSnapType.NONE;
                                break;
                            }
                            snapType++;
                        }
                        if (Event.current.keyCode == KeyCode.DownArrow)
                        {
                            relativeTexturePositon += snapMovement[(int)snapType];
                        }
                        if (Event.current.keyCode == KeyCode.UpArrow)
                        {
                            relativeTexturePositon -= snapMovement[(int)snapType];
                        }
                        break;
                    }
            }

            if(relativeTexturePositon < 0)
            {
                relativeTexturePositon = textureHeight;
            }
            if(relativeTexturePositon > textureHeight)
            {
                relativeTexturePositon = 0;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Note Snap: {snapType.ToString()}");
            if (GUILayout.Button("Toggle"))
            {
                if (playObject.GetComponent<AudioSource>().isPlaying == false)
                {
                    playObject.GetComponent<AudioSource>().Play();
                }
                else
                {
                    playObject.GetComponent<AudioSource>().Stop();
                }
            }
            playRate = EditorGUILayout.Slider(playRate, 0.0f, 2.0f);
            EditorGUILayout.EndHorizontal();

            if (playObject.GetComponent<AudioSource>().isPlaying)
            {
                relativeTexturePositon = playObject.GetComponent<AudioSource>().timeSamples * songPiece.song.channels * samplesModifier;
            }

            scrollView = GUI.BeginScrollView(new Rect(0, 60, position.width, position.height-60), scrollView, new Rect(0, 0, 0, textureHeight), false, true);
            int currHeightOffset = 0;
            for(int i = 0; i < waveFormTextures.Length; i++)
            {
                EditorGUI.DrawPreviewTexture(new Rect(0, currHeightOffset, position.width, waveFormTextures[i].height), waveFormTextures[i]);
                currHeightOffset += waveFormTextures[i].height;
            }
            DrawSnapLines();
            DrawUILine(Color.red, relativeTexturePositon, 2, 10);
            GUI.EndScrollView();
        }

        private void DrawSnapLines()
        {
            float interval = 5;
            if(snapType == NoteSnapType.NONE)
            {
                return;
            }

            float secondsPerBeat = 60.0f / songPiece.bpm;
            //Debug.Log($"Sample Rate: {songPiece.song.frequency}, Samples per beat: {secondsPerBeat * songPiece.song.frequency}");
            int samplesPerBeat = (int)(secondsPerBeat * songPiece.song.frequency);
            switch (snapType)
            {
                case NoteSnapType.WHOLE:
                    interval = secondsPerBeat * songPiece.song.frequency * samplesModifier;
                    break;
                case NoteSnapType.HALF:
                    interval = ((secondsPerBeat / 2.0f) * songPiece.song.frequency * songPiece.song.channels * samplesModifier);
                    break;
                case NoteSnapType.QUARTER:
                    interval = ((secondsPerBeat / 4.0f) * songPiece.song.frequency * songPiece.song.channels * samplesModifier);
                    break;
            }

            DrawUILine(new Color(1, 1, 1, 0.5f), interval);
            /*for(int i = 0; i < textureHeight/interval; i++)
            {
                DrawUILine(new Color(1, 1, 1, 0.5f), interval * i, 1, 0);
            }*/
        }

        private void CalculateMoveNumbers()
        {
            snapMovement[0] = 1;
            snapMovement[1] = 5;
        }

        public void GenerateWaveFormTexture()
        {            
            textureHeight = 0;

            int totalTextureHeight = (int)(songPiece.song.samples * samplesModifier);
            int totalSections = (totalTextureHeight / 4096) + 1;
            int samplesPerSection = (songPiece.song.samples) / totalSections;
            Debug.Log($"Total height: {totalTextureHeight}, Totla Sections: {totalSections}, Samples Per Section: {samplesPerSection}" +
                $", Test Total: {(float)songPiece.song.samples / (float)songPiece.song.frequency}");
            
            waveFormTextures = new Texture2D[totalSections];

            for(int i = 0; i < totalSections; i++)
            {
                int sectionTextureHeight = 4096;

                waveFormTextures[i] = PaintWaveformSpectrum(
                    songPiece.song,
                    samplesPerSection * i + 1,
                    samplesPerSection,
                    2048,
                    sectionTextureHeight,
                    Color.green);
                textureHeight += sectionTextureHeight;
            }
            Debug.Log($"SPP: {samplesPerPixel}");
        }

        public Texture2D PaintWaveformSpectrum(AudioClip audio, int samplesOffset, int samplesCount, int width, int height, Color col)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            float[] samples = new float[samplesCount];
            float[] waveform = new float[height];
            audio.GetData(samples, samplesOffset);
            samplesPerPixel = (samplesCount / height) + 1;
            int s = 0;
            for (int i = 0; i < samplesCount; i += samplesPerPixel)
            {
                waveform[s] = Mathf.Abs(samples[i]) * audio.channels;
                s++;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tex.SetPixel(x, y, Color.black);
                }
            }
            
            for(int x = 0; x < waveform.Length; x++)
            {
                for(int y = 0; y <= waveform[x] * ((float)width * .75f); y++)
                {
                    tex.SetPixel((width / 2) + y, x, col);
                    tex.SetPixel((width / 2) - y, x, col);
                }
            }
            tex.Apply();
            return tex;
        }

        public void DrawUILine(Color color, float yPos, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y = yPos;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
    }
}