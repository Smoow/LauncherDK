using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace LauncherDK
{
    /// <summary>
    /// Interaction logic for Config.xaml
    /// </summary>
    public partial class Config : Window
    {
        public short m_Version;//Patch
        public short m_ResIndex;//ResIndex
        public short m_Smooth;//Smooth
        public short m_Sound;//Sond
        public short m_Music;//Music
        public short m_NotUsed;//NotUsed
        public short m_Brilho;//Brilho
        public short m_Cursor;//Cursor
        public short m_PlayDemo;//PlayDemo
        public short m_FullScreen;//FullScreen
        public short m_UIVersion;//UiVersion
        public short m_CameraRout;//CamerRout
        public short m_DxtEnabled;//DxtEnable
        public short m_KeyType;//KeyType
        public short m_CameraView;//CameraView

        public Config()
        {
            InitializeComponent();
            ReadConfigFile();
        }

        public void ReadConfigFile()
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open("./config.bin", FileMode.Open)))
                {

                    do
                    {
                        m_Version = reader.ReadInt16();
                        m_ResIndex = reader.ReadInt16();
                        m_Smooth = reader.ReadInt16();
                        m_Sound = reader.ReadInt16();
                        m_Music = reader.ReadInt16();
                        m_NotUsed = reader.ReadInt16();
                        m_Brilho = reader.ReadInt16();
                        m_Cursor = reader.ReadInt16();
                        m_PlayDemo = reader.ReadInt16();
                        m_FullScreen = reader.ReadInt16();
                        m_UIVersion = reader.ReadInt16();
                        m_CameraRout = reader.ReadInt16();
                        m_DxtEnabled = reader.ReadInt16();
                        m_KeyType = reader.ReadInt16();
                        m_CameraView = reader.ReadInt16();


                        //if (m_ResIndex == 0)
                        //    m_ResIndex = 1;

                    } while (reader.BaseStream.Position < reader.BaseStream.Length);
                }

                // Colocando os valores atuais nas Boxes/Sliders.
                resolutionBox.SelectedIndex = m_ResIndex - 1;
                fullscreenBox.SelectedIndex = m_FullScreen;
                sliderMusic.Value = m_Music;
                sliderSound.Value = m_Sound;
                rotationBox.SelectedIndex = m_CameraRout;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n Src:" + ex.Source + "\nData:" + ex.Data);
            }
        }

        private void SaveConfig(object sender, RoutedEventArgs e)
        {
            // 1280x1024 => 9
            if (m_ResIndex == 6)
            {
                m_ResIndex = 9;
            }

            using (BinaryWriter writer = new BinaryWriter(File.Open("./config.bin", FileMode.Create)))
            {
                try
                {

                    writer.Write(m_Version);
                    writer.Write(m_ResIndex);
                    writer.Write(m_Smooth);
                    writer.Write(m_Sound);
                    writer.Write(m_Music);
                    writer.Write(m_NotUsed);
                    writer.Write(m_Brilho);
                    writer.Write(m_Cursor);
                    writer.Write(m_PlayDemo);
                    writer.Write(m_FullScreen);
                    writer.Write(m_UIVersion);
                    writer.Write(m_CameraRout);
                    writer.Write(m_DxtEnabled);
                    writer.Write(m_KeyType);
                    writer.Write(m_CameraView);
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n Src:" + ex.Source + "\nData:" + ex.Data);
                }

                this.Close();
            }
        }

        private void resolutionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_ResIndex = (short)(resolutionBox.SelectedIndex + 1);
        }

        private void fullscreenBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_FullScreen = (short)fullscreenBox.SelectedIndex;
        }

        private void sliderMusic_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_Music = (short)sliderMusic.Value;
            labelVolumeMusic.Content = "(" + Math.Round(sliderMusic.Value, 0) + ")";
        }

        private void sliderSound_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_Sound = (short)sliderSound.Value;
            labelVolumeEffects.Content = "(" + Math.Round(sliderSound.Value, 0) + ")";
        }

        private void rotationBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_CameraRout = (short)rotationBox.SelectedIndex;

        }

        private void MuteMusic(object sender, RoutedEventArgs e)
        {
            sliderMusic.Value = 0;
            m_Music = 0;
        }

        private void MuteEffects(object sender, RoutedEventArgs e)
        {
            sliderSound.Value = 0;
            m_Sound = 0;
        }
    }
}
