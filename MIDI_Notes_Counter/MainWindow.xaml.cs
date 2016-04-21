using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using NAudio.Midi;

namespace MIDI_Notes_Counter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        List<Scale> allModes = new List<Scale>();
        public MainWindow()
        {
            InitializeComponent();

            allModes.Add(new Scale { Name = "C major", Notes = new string[] { "C", "D", "E", "F", "G", "A", "B" } });
            allModes.Add(new Scale { Name = "A minor", Notes = new string[] { "C", "D", "E", "F", "G", "A", "B" } });
        }


        class Scale
        {
            public string[] Notes { get; set; }
            public string Name { get; set; }
        }


        class Note  //класс "Нота", для представления нот
        {
            public string Name { get; set; } //свойство ноты "имя"
            public int Count { get; set; }   //свойство ноты "встречаемость"
        }


        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))        //если событие вызвано перетаскиванием файлов
            {                  
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);    //получаем список всех перетащеных файлов

                if (files.Count() != 1)                              //если файл не 1
                {
                    MessageBox.Show("Поддерживается обработка только одного файла."); //то говорим, что можно только 1 (=
                    return;                                                           //уходим
                }


                try                                                                     //пытаемся считать файл
                {
                    MidiFile midi = new MidiFile(files[0]);                             //пытаемся считать файл как MIDI
                    
                    //var tracksCount = midi.Tracks;
                    //var events = midi.Events.Count();

                    var allNotes = new List<Note>();                                    //создаём список, куда будем записывать проигранные ноты и считать их

                    foreach (MidiEvent currEvent in midi.Events[1])                     //для каждого MIDI события в файле
                    {
                        if (currEvent.CommandCode == MidiCommandCode.NoteOn)            //если событие является нажатием ноты
                        {
                            var curr_note = (NoteOnEvent)currEvent;                     //то преобразуем событие в ноту

                            var noteName = curr_note.NoteName;                          //получаем обозначение (будущее "имя") ноты                            

                            if(!allNotes.Exists(x => x.Name == noteName))                //если встретилась новая нота (не добавляли в список нот)
                            {
                                allNotes.Add(new Note { Name = noteName, Count = 1 });   //то добавляем текущую ноту с числом встречаемости = 1
                            }
                            else                                                         //если такая нота уже есть в списке всех нот
                            {
                                var index = allNotes.FindIndex(x => x.Name == noteName); //то находим её индекс (позицию) в списке

                                allNotes[index].Count++;                                 //и увеличиваем встречаемость ноты на 1
                            }

                        }//if NoteOn                        
                    }//foreach

                    
                    
                    var notesSortedByCount = allNotes.OrderByDescending(x => x.Count).ToList(); //сортируем список по встречаемости нот и записываем в соотв. переменную

                    NotesStats_TextBox.Text = String.Format("Stats for \"{0}\":\n\n", GetFileNameFromFullPath(files[0])); //создаём "шапку", выводим для какого файла (имя) статистика
                    NotesStats_TextBox.Text += String.Format("{0}{1,7}\n", "Note", "Count");                              //выводим название полей

                    foreach (Note note in notesSortedByCount) //для каждой ноты в отсортированном списке
                    {
                        if(note.Name.Length == 2)                   //если обозначение ноты занимает 2 символа
                            NotesStats_TextBox.Text += String.Format("{0}:{1,7}\n", note.Name, note.Count); //выводим текущую ноту с числом встречаемости с форматированием под двухсимвольное обозначение ноты
                        else
                            NotesStats_TextBox.Text += String.Format("{0}:{1,6}\n", note.Name, note.Count); //--//-- под трёхсимвольное
                    }

                }//try read MIDI                
                catch
                {
                    NotesStats_TextBox.Text = "";

                    MessageBox.Show("Неподдерживаемый тип файла!");
                    return;
                }
            }//if GetDataPresent
        }//Drop Event


        static string GetFileNameFromFullPath(string filePath) //метод для получения имени файла из полного пути до файла
        {
            for(int index = filePath.Length - 1; index > 0; index--)                     //начинаем просмотр символов в пути к файлу
            {
                if (filePath[index] == '/' || filePath[index] == '\\')                   //как только наткнулись на символ, отделяющий имя файла
                    return filePath.Substring(index + 1, filePath.Length - index - 1);   //возвращаем подстроку, которая начинается от следущего символа, до конца строки
            }

            return ""; //если такого разграничительного символа не встретилось, то возвращаем пустую строку
        }
    }
}
