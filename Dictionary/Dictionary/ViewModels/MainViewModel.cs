using Dictionary.Commands;
using Dictionary.DataClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Dictionary.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Nyelvválasztó ComboBoxok
        private ObservableCollection<string> languageComboBox = new ObservableCollection<string> { "", "", "" };
        private ObservableCollection<string> languageComboBoxSecond = new ObservableCollection<string> { "", "", "" };
        private string languageComboBoxSelectedItem;
        private string languageComboBoxSecondSelectedItem;
        private string wordTyper;
        private HttpClient httpClient;
        public ICommand TranslateCommand { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            // Fordítás parancs inicializálása
            TranslateCommand = new TranslateButtonCommand(Translate);
            InitializeAsync();
        }

        // Inicializálás aszinkron módon
        private async Task InitializeAsync()
        {
            httpClient = new HttpClient();
            await LoadLanguages();
        }

        // Tulajdonságok
        public ObservableCollection<string> LanguageComboBoxList => languageComboBox;
        public ObservableCollection<string> LanguageComboBoxSecondList => languageComboBoxSecond;

        public string LanguageComboBoxSelectedItemText
        {
            get => languageComboBoxSelectedItem;
            set { languageComboBoxSelectedItem = value; OnPropertyChanged(); }
        }

        public string LanguageComboBoxSecondSelectedItemText
        {
            get => languageComboBoxSecondSelectedItem;
            set { languageComboBoxSecondSelectedItem = value; OnPropertyChanged(); }
        }

        public string WordTyperText
        {
            get => wordTyper;
            set { wordTyper = value; OnPropertyChanged(); }
        }

        // Segédfüggvények
        private string GetAPICallString() => $"{LanguageComboBoxSelectedItemText}-{LanguageComboBoxSecondSelectedItemText}";
        private string GetUserInput() => WordTyperText;

        // Nyelvlista betöltése
        private async Task LoadLanguages()
        {
            try
            {
                string url = "https://dictionary.yandex.net/api/v1/dicservice.json/getLangs?key=dict.1.1.20230520T220053Z.86bb28ae338d15a0.7fdc928081b1d6aecbfee4dcc26fbc535f42c426";
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                // JSON válasz feldolgozása
                List<string> languageCodes = JsonConvert.DeserializeObject<List<string>>(jsonResponse);
                languageComboBox.Clear();
                languageComboBoxSecond.Clear();

                HashSet<string> FirstPart = new HashSet<string>();
                HashSet<string> SecondPart = new HashSet<string>();

                if (languageCodes != null)
                {
                    foreach (var languageCode in languageCodes)
                    {
                        string[] parts = languageCode.Split('-');
                        if (parts.Length == 2)
                        {
                            FirstPart.Add(parts[0]);
                            SecondPart.Add(parts[1]);
                        }
                    }
                }

                // Nyelvválasztó ComboBoxok frissítése
                foreach (var firstPart in FirstPart)
                {
                    languageComboBox.Add(firstPart);
                }

                foreach (var secondPart in SecondPart)
                {
                    languageComboBoxSecond.Add(secondPart);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Fordítás végrehajtása
        private async void Translate()
        {
            // Fordításhoz szükséges adatok lekérése
            string languageCode = GetAPICallString();
            string inputText = GetUserInput();

            // Üres bemenetek ellenőrzése
            if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(languageCode))
                return;

            string url = $"https://dictionary.yandex.net/api/v1/dicservice.json/lookup?key=dict.1.1.20230520T220053Z.86bb28ae338d15a0.7fdc928081b1d6aecbfee4dcc26fbc535f42c426&lang={languageCode}&text={inputText}";

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Fordítási válasz feldolgozása
                if (jsonResponse.Contains("\"code\": 501") && jsonResponse.Contains("\"message\": \"The specified language is not supported\""))
                {
                    ShowDialog("Translation is not supported, please try a valid pair of Languages", "Invalid Parameters");
                    return;
                }

                ProcessTranslationResponse(jsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Fordítási válasz feldolgozása
        private void ProcessTranslationResponse(string jsonResponse)
        {
            LanguageResponse languageresponse = JsonConvert.DeserializeObject<LanguageResponse>(jsonResponse);

            if (languageresponse?.Def?.Count > 0 && languageresponse.Def[0]?.Tr?.Count > 0 && languageresponse.Def[0].Tr[0]?.Syn?.Count > 0)
            {
                string translateddefinition = languageresponse.Def[0].Tr[0].Text;
                string synonymdefinition = languageresponse.Def[0].Tr[0].Syn[0].Text;

                ShowDialog(translateddefinition, synonymdefinition);
            }
        }

        // Dialógusablak megjelenítése
        private async void ShowDialog(string translateddefinition, string synonymdefinition)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Translation",
                Content = $"Definition: {translateddefinition}\nSynonym: {synonymdefinition}",
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }

        // Tulajdonság megváltozásának eseménykezelője
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
