#nullable disable
using System.Collections.Generic;

namespace MakoMiniPlayer
{
    public static class Translations
    {
        public static string CurrentLang = "hr";

        private static readonly Dictionary<string, Dictionary<string, string>> T = new()
        {
            ["play"] = new()
            {
                ["hr"] = "Play",
                ["en"] = "Play",
                ["de"] = "Wiedergabe",
                ["fr"] = "Lecture",
                ["es"] = "Reproducir",
                ["it"] = "Riproduci",
                ["pt"] = "Reproduzir",
                ["ru"] = "Воспроизвести",
                ["pl"] = "Odtwórz",
                ["cs"] = "Přehrát",
                ["nl"] = "Afspelen",
                ["sv"] = "Spela",
                ["tr"] = "Oynat"
            },
            ["pause"] = new()
            {
                ["hr"] = "Pauza",
                ["en"] = "Pause",
                ["de"] = "Pause",
                ["fr"] = "Pause",
                ["es"] = "Pausa",
                ["it"] = "Pausa",
                ["pt"] = "Pausa",
                ["ru"] = "Пауза",
                ["pl"] = "Pauza",
                ["cs"] = "Pozastavit",
                ["nl"] = "Pauze",
                ["sv"] = "Paus",
                ["tr"] = "Duraklat"
            },
            ["stop"] = new()
            {
                ["hr"] = "Stop",
                ["en"] = "Stop",
                ["de"] = "Stopp",
                ["fr"] = "Arrêt",
                ["es"] = "Detener",
                ["it"] = "Ferma",
                ["pt"] = "Parar",
                ["ru"] = "Стоп",
                ["pl"] = "Zatrzymaj",
                ["cs"] = "Zastavit",
                ["nl"] = "Stoppen",
                ["sv"] = "Stoppa",
                ["tr"] = "Durdur"
            },
            ["minus5"] = new()
            {
                ["hr"] = "-5 minuta",
                ["en"] = "-5 minutes",
                ["de"] = "-5 Minuten",
                ["fr"] = "-5 minutes",
                ["es"] = "-5 minutos",
                ["it"] = "-5 minuti",
                ["pt"] = "-5 minutos",
                ["ru"] = "-5 минут",
                ["pl"] = "-5 minut",
                ["cs"] = "-5 minut",
                ["nl"] = "-5 minuten",
                ["sv"] = "-5 minuter",
                ["tr"] = "-5 dakika"
            },
            ["plus5"] = new()
            {
                ["hr"] = "+5 minuta",
                ["en"] = "+5 minutes",
                ["de"] = "+5 Minuten",
                ["fr"] = "+5 minutes",
                ["es"] = "+5 minutos",
                ["it"] = "+5 minuti",
                ["pt"] = "+5 minutos",
                ["ru"] = "+5 минут",
                ["pl"] = "+5 minut",
                ["cs"] = "+5 minut",
                ["nl"] = "+5 minuten",
                ["sv"] = "+5 minuter",
                ["tr"] = "+5 dakika"
            },
            ["loadSub"] = new()
            {
                ["hr"] = "Učitaj titl sa diska",
                ["en"] = "Load subtitle from disk",
                ["de"] = "Untertitel von Festplatte laden",
                ["fr"] = "Charger les sous-titres",
                ["es"] = "Cargar subtítulos del disco",
                ["it"] = "Carica sottotitoli dal disco",
                ["pt"] = "Carregar legenda do disco",
                ["ru"] = "Загрузить субтитры с диска",
                ["pl"] = "Wczytaj napisy z dysku",
                ["cs"] = "Načíst titulky z disku",
                ["nl"] = "Ondertitel van schijf laden",
                ["sv"] = "Ladda undertext från disk",
                ["tr"] = "Diskten altyazı yükle"
            },
            ["subStyle"] = new()
            {
                ["hr"] = "Stil titlova",
                ["en"] = "Subtitle style",
                ["de"] = "Untertitelstil",
                ["fr"] = "Style des sous-titres",
                ["es"] = "Estilo de subtítulos",
                ["it"] = "Stile sottotitoli",
                ["pt"] = "Estilo da legenda",
                ["ru"] = "Стиль субтитров",
                ["pl"] = "Styl napisów",
                ["cs"] = "Styl titulků",
                ["nl"] = "Ondertitelstijl",
                ["sv"] = "Undertextstil",
                ["tr"] = "Altyazı stili"
            },
            ["subOff"] = new()
            {
                ["hr"] = "Isključi titlove",
                ["en"] = "Turn off subtitles",
                ["de"] = "Untertitel ausschalten",
                ["fr"] = "Désactiver les sous-titres",
                ["es"] = "Desactivar subtítulos",
                ["it"] = "Disattiva sottotitoli",
                ["pt"] = "Desativar legendas",
                ["ru"] = "Выключить субтитры",
                ["pl"] = "Wyłącz napisy",
                ["cs"] = "Vypnout titulky",
                ["nl"] = "Ondertitels uit",
                ["sv"] = "Stäng av undertext",
                ["tr"] = "Altyazıyı kapat"
            },
            ["embeddedSub"] = new()
            {
                ["hr"] = "Ugrađeni titlovi:",
                ["en"] = "Embedded subtitles:",
                ["de"] = "Eingebettete Untertitel:",
                ["fr"] = "Sous-titres intégrés :",
                ["es"] = "Subtítulos integrados:",
                ["it"] = "Sottotitoli incorporati:",
                ["pt"] = "Legendas incorporadas:",
                ["ru"] = "Встроенные субтитры:",
                ["pl"] = "Wbudowane napisy:",
                ["cs"] = "Vložené titulky:",
                ["nl"] = "Ingebedde ondertitels:",
                ["sv"] = "Inbäddade undertexter:",
                ["tr"] = "Gömülü altyazılar:"
            },
            ["audioEq"] = new()
            {
                ["hr"] = "Audio EQ",
                ["en"] = "Audio EQ",
                ["de"] = "Audio-EQ",
                ["fr"] = "Égaliseur audio",
                ["es"] = "Ecualizador",
                ["it"] = "Equalizzatore",
                ["pt"] = "Equalizador",
                ["ru"] = "Эквалайзер",
                ["pl"] = "Korektor",
                ["cs"] = "Ekvalizér",
                ["nl"] = "Audio-EQ",
                ["sv"] = "Ljud-EQ",
                ["tr"] = "Ses EQ"
            },
            ["mediaInfo"] = new()
            {
                ["hr"] = "Video/Audio info",
                ["en"] = "Video/Audio info",
                ["de"] = "Video/Audio-Info",
                ["fr"] = "Infos vidéo/audio",
                ["es"] = "Info de video/audio",
                ["it"] = "Info video/audio",
                ["pt"] = "Info de vídeo/áudio",
                ["ru"] = "Инфо видео/аудио",
                ["pl"] = "Info wideo/audio",
                ["cs"] = "Info videa/audia",
                ["nl"] = "Video/audio-info",
                ["sv"] = "Video/ljud-info",
                ["tr"] = "Video/Ses bilgisi"
            },
            ["language"] = new()
            {
                ["hr"] = "Jezik sučelja",
                ["en"] = "Interface language",
                ["de"] = "Sprache",
                ["fr"] = "Langue",
                ["es"] = "Idioma",
                ["it"] = "Lingua",
                ["pt"] = "Idioma",
                ["ru"] = "Язык интерфейса",
                ["pl"] = "Język interfejsu",
                ["cs"] = "Jazyk rozhraní",
                ["nl"] = "Taal",
                ["sv"] = "Språk",
                ["tr"] = "Arayüz dili"
            },
            ["screenshot"] = new()
            {
                ["hr"] = "Screenshot",
                ["en"] = "Screenshot",
                ["de"] = "Bildschirmfoto",
                ["fr"] = "Capture d'écran",
                ["es"] = "Captura",
                ["it"] = "Schermata",
                ["pt"] = "Captura de tela",
                ["ru"] = "Снимок экрана",
                ["pl"] = "Zrzut ekranu",
                ["cs"] = "Snímek obrazovky",
                ["nl"] = "Schermafbeelding",
                ["sv"] = "Skärmdump",
                ["tr"] = "Ekran görüntüsü"
            },
            ["fullscreen"] = new()
            {
                ["hr"] = "Cijeli ekran",
                ["en"] = "Fullscreen",
                ["de"] = "Vollbild",
                ["fr"] = "Plein écran",
                ["es"] = "Pantalla completa",
                ["it"] = "Schermo intero",
                ["pt"] = "Tela cheia",
                ["ru"] = "Полный экран",
                ["pl"] = "Pełny ekran",
                ["cs"] = "Celá obrazovka",
                ["nl"] = "Volledig scherm",
                ["sv"] = "Helskärm",
                ["tr"] = "Tam ekran"
            },
            ["exitFullscreen"] = new()
            {
                ["hr"] = "Izlaz iz fullscreena",
                ["en"] = "Exit fullscreen",
                ["de"] = "Vollbild beenden",
                ["fr"] = "Quitter le plein écran",
                ["es"] = "Salir de pantalla completa",
                ["it"] = "Esci da schermo intero",
                ["pt"] = "Sair da tela cheia",
                ["ru"] = "Выйти из полного экрана",
                ["pl"] = "Wyjdź z pełnego ekranu",
                ["cs"] = "Ukončit celou obrazovku",
                ["nl"] = "Volledig scherm verlaten",
                ["sv"] = "Avsluta helskärm",
                ["tr"] = "Tam ekrandan çık"
            },
            ["onTop"] = new()
            {
                ["hr"] = "Uvijek na vrhu",
                ["en"] = "Always on top",
                ["de"] = "Immer im Vordergrund",
                ["fr"] = "Toujours visible",
                ["es"] = "Siempre visible",
                ["it"] = "Sempre in primo piano",
                ["pt"] = "Sempre no topo",
                ["ru"] = "Поверх всех окон",
                ["pl"] = "Zawsze na wierzchu",
                ["cs"] = "Vždy navrchu",
                ["nl"] = "Altijd bovenaan",
                ["sv"] = "Alltid överst",
                ["tr"] = "Her zaman üstte"
            },
            ["openFile"] = new()
            {
                ["hr"] = "Otvori fajl",
                ["en"] = "Open file",
                ["de"] = "Datei öffnen",
                ["fr"] = "Ouvrir un fichier",
                ["es"] = "Abrir archivo",
                ["it"] = "Apri file",
                ["pt"] = "Abrir arquivo",
                ["ru"] = "Открыть файл",
                ["pl"] = "Otwórz plik",
                ["cs"] = "Otevřít soubor",
                ["nl"] = "Bestand openen",
                ["sv"] = "Öppna fil",
                ["tr"] = "Dosya aç"
            },
            ["buyCoffee"] = new()
            {
                ["hr"] = "Buy me a coffee",
                ["en"] = "Buy me a coffee",
                ["de"] = "Spendiere einen Kaffee",
                ["fr"] = "Offre-moi un café",
                ["es"] = "Invítame un café",
                ["it"] = "Offrimi un caffè",
                ["pt"] = "Pague-me um café",
                ["ru"] = "Купить мне кофе",
                ["pl"] = "Postaw mi kawę",
                ["cs"] = "Kup mi kávu",
                ["nl"] = "Trakteer op koffie",
                ["sv"] = "Bjud på kaffe",
                ["tr"] = "Bana kahve ısmarla"
            },
            ["about"] = new()
            {
                ["hr"] = "O programu",
                ["en"] = "About",
                ["de"] = "Über",
                ["fr"] = "À propos",
                ["es"] = "Acerca de",
                ["it"] = "Informazioni",
                ["pt"] = "Sobre",
                ["ru"] = "О программе",
                ["pl"] = "O programie",
                ["cs"] = "O programu",
                ["nl"] = "Over",
                ["sv"] = "Om",
                ["tr"] = "Hakkında"
            },
            ["closePlayer"] = new()
            {
                ["hr"] = "Zatvori player",
                ["en"] = "Close player",
                ["de"] = "Player schließen",
                ["fr"] = "Fermer le lecteur",
                ["es"] = "Cerrar reproductor",
                ["it"] = "Chiudi lettore",
                ["pt"] = "Fechar reprodutor",
                ["ru"] = "Закрыть плеер",
                ["pl"] = "Zamknij odtwarzacz",
                ["cs"] = "Zavřít přehrávač",
                ["nl"] = "Speler sluiten",
                ["sv"] = "Stäng spelaren",
                ["tr"] = "Oynatıcıyı kapat"
            },
            ["subtitles"] = new()
            {
                ["hr"] = "Titlovi",
                ["en"] = "Subtitles",
                ["de"] = "Untertitel",
                ["fr"] = "Sous-titres",
                ["es"] = "Subtítulos",
                ["it"] = "Sottotitoli",
                ["pt"] = "Legendas",
                ["ru"] = "Субтитры",
                ["pl"] = "Napisy",
                ["cs"] = "Titulky",
                ["nl"] = "Ondertitels",
                ["sv"] = "Undertexter",
                ["tr"] = "Altyazılar"
            },
            ["open"] = new()
            {
                ["hr"] = "Otvori",
                ["en"] = "Open",
                ["de"] = "Öffnen",
                ["fr"] = "Ouvrir",
                ["es"] = "Abrir",
                ["it"] = "Apri",
                ["pt"] = "Abrir",
                ["ru"] = "Открыть",
                ["pl"] = "Otwórz",
                ["cs"] = "Otevřít",
                ["nl"] = "Openen",
                ["sv"] = "Öppna",
                ["tr"] = "Aç"
            },
            ["loadFromDisk"] = new()
            {
                ["hr"] = "Učitaj sa diska",
                ["en"] = "Load from disk",
                ["de"] = "Von Festplatte laden",
                ["fr"] = "Charger depuis le disque",
                ["es"] = "Cargar del disco",
                ["it"] = "Carica dal disco",
                ["pt"] = "Carregar do disco",
                ["ru"] = "Загрузить с диска",
                ["pl"] = "Wczytaj z dysku",
                ["cs"] = "Načíst z disku",
                ["nl"] = "Van schijf laden",
                ["sv"] = "Ladda från disk",
                ["tr"] = "Diskten yükle"
            },
            ["resume"] = new()
            {
                ["hr"] = "Nastavi gledanje",
                ["en"] = "Resume watching",
                ["de"] = "Weiterschauen",
                ["fr"] = "Reprendre",
                ["es"] = "Continuar viendo",
                ["it"] = "Riprendi visione",
                ["pt"] = "Continuar assistindo",
                ["ru"] = "Продолжить просмотр",
                ["pl"] = "Wznów oglądanie",
                ["cs"] = "Pokračovat ve sledování",
                ["nl"] = "Verder kijken",
                ["sv"] = "Fortsätt titta",
                ["tr"] = "İzlemeye devam et"
            },
            ["resumeFrom"] = new()
            {
                ["hr"] = "Nastavi od",
                ["en"] = "Resume from",
                ["de"] = "Fortsetzen ab",
                ["fr"] = "Reprendre à",
                ["es"] = "Continuar desde",
                ["it"] = "Riprendi da",
                ["pt"] = "Continuar de",
                ["ru"] = "Продолжить с",
                ["pl"] = "Wznów od",
                ["cs"] = "Pokračovat od",
                ["nl"] = "Hervatten vanaf",
                ["sv"] = "Fortsätt från",
                ["tr"] = "Şuradan devam et"
            },
            ["langChanged"] = new()
            {
                ["hr"] = "Jezik promijenjen.",
                ["en"] = "Language changed.",
                ["de"] = "Sprache geändert.",
                ["fr"] = "Langue changée.",
                ["es"] = "Idioma cambiado.",
                ["it"] = "Lingua cambiata.",
                ["pt"] = "Idioma alterado.",
                ["ru"] = "Язык изменён.",
                ["pl"] = "Język zmieniony.",
                ["cs"] = "Jazyk změněn.",
                ["nl"] = "Taal gewijzigd.",
                ["sv"] = "Språk ändrat.",
                ["tr"] = "Dil değiştirildi."
            },
            ["resumeQuestion"] = new()
            {
                ["hr"] = "Nastavi od {0}?",
                ["en"] = "Resume from {0}?",
                ["de"] = "Fortsetzen ab {0}?",
                ["fr"] = "Reprendre à {0} ?",
                ["es"] = "¿Continuar desde {0}?",
                ["it"] = "Riprendere da {0}?",
                ["pt"] = "Continuar de {0}?",
                ["ru"] = "Продолжить с {0}?",
                ["pl"] = "Wznowić od {0}?",
                ["cs"] = "Pokračovat od {0}?",
                ["nl"] = "Hervatten vanaf {0}?",
                ["sv"] = "Fortsätt från {0}?",
                ["tr"] = "{0} konumundan devam edilsin mi?"
            },
            ["screenshotSaved"] = new()
            {
                ["hr"] = "Screenshot sačuvan:",
                ["en"] = "Screenshot saved:",
                ["de"] = "Bildschirmfoto gespeichert:",
                ["fr"] = "Capture enregistrée :",
                ["es"] = "Captura guardada:",
                ["it"] = "Schermata salvata:",
                ["pt"] = "Captura salva:",
                ["ru"] = "Снимок сохранён:",
                ["pl"] = "Zrzut zapisany:",
                ["cs"] = "Snímek uložen:",
                ["nl"] = "Schermafbeelding opgeslagen:",
                ["sv"] = "Skärmdump sparad:",
                ["tr"] = "Ekran görüntüsü kaydedildi:"
            },
            ["error"] = new()
            {
                ["hr"] = "Greška:",
                ["en"] = "Error:",
                ["de"] = "Fehler:",
                ["fr"] = "Erreur :",
                ["es"] = "Error:",
                ["it"] = "Errore:",
                ["pt"] = "Erro:",
                ["ru"] = "Ошибка:",
                ["pl"] = "Błąd:",
                ["cs"] = "Chyba:",
                ["nl"] = "Fout:",
                ["sv"] = "Fel:",
                ["tr"] = "Hata:"
            },
            ["subNotFound"] = new()
            {
                ["hr"] = "Subtitle fajl nije pronađen:",
                ["en"] = "Subtitle file not found:",
                ["de"] = "Untertiteldatei nicht gefunden:",
                ["fr"] = "Fichier de sous-titres introuvable :",
                ["es"] = "Archivo de subtítulos no encontrado:",
                ["it"] = "File sottotitoli non trovato:",
                ["pt"] = "Arquivo de legenda não encontrado:",
                ["ru"] = "Файл субтитров не найден:",
                ["pl"] = "Nie znaleziono pliku napisów:",
                ["cs"] = "Soubor titulků nenalezen:",
                ["nl"] = "Ondertitelbestand niet gevonden:",
                ["sv"] = "Undertextfil hittades inte:",
                ["tr"] = "Altyazı dosyası bulunamadı:"
            },
            ["subEmpty"] = new()
            {
                ["hr"] = "Fajl je prazan ili format nije podržan.",
                ["en"] = "File is empty or format not supported.",
                ["de"] = "Datei ist leer oder Format nicht unterstützt.",
                ["fr"] = "Fichier vide ou format non pris en charge.",
                ["es"] = "Archivo vacío o formato no compatible.",
                ["it"] = "File vuoto o formato non supportato.",
                ["pt"] = "Arquivo vazio ou formato não suportado.",
                ["ru"] = "Файл пуст или формат не поддерживается.",
                ["pl"] = "Plik jest pusty lub format nieobsługiwany.",
                ["cs"] = "Soubor je prázdný nebo formát není podporován.",
                ["nl"] = "Bestand is leeg of formaat niet ondersteund.",
                ["sv"] = "Filen är tom eller formatet stöds inte.",
                ["tr"] = "Dosya boş veya format desteklenmiyor."
            },
            ["noFile"] = new()
            {
                ["hr"] = "Nema učitanog fajla.",
                ["en"] = "No file loaded.",
                ["de"] = "Keine Datei geladen.",
                ["fr"] = "Aucun fichier chargé.",
                ["es"] = "Ningún archivo cargado.",
                ["it"] = "Nessun file caricato.",
                ["pt"] = "Nenhum arquivo carregado.",
                ["ru"] = "Файл не загружен.",
                ["pl"] = "Nie wczytano pliku.",
                ["cs"] = "Není načten žádný soubor.",
                ["nl"] = "Geen bestand geladen.",
                ["sv"] = "Ingen fil laddad.",
                ["tr"] = "Dosya yüklenmedi."
            },
            ["duration"] = new()
            {
                ["hr"] = "Trajanje:",
                ["en"] = "Duration:",
                ["de"] = "Dauer:",
                ["fr"] = "Durée :",
                ["es"] = "Duración:",
                ["it"] = "Durata:",
                ["pt"] = "Duração:",
                ["ru"] = "Длительность:",
                ["pl"] = "Czas trwania:",
                ["cs"] = "Délka:",
                ["nl"] = "Duur:",
                ["sv"] = "Längd:",
                ["tr"] = "Süre:"
            },
            ["size"] = new()
            {
                ["hr"] = "Veličina:",
                ["en"] = "Size:",
                ["de"] = "Größe:",
                ["fr"] = "Taille :",
                ["es"] = "Tamaño:",
                ["it"] = "Dimensione:",
                ["pt"] = "Tamanho:",
                ["ru"] = "Размер:",
                ["pl"] = "Rozmiar:",
                ["cs"] = "Velikost:",
                ["nl"] = "Grootte:",
                ["sv"] = "Storlek:",
                ["tr"] = "Boyut:"
            },
            ["audioTracks"] = new()
            {
                ["hr"] = "Audio trackovi:",
                ["en"] = "Audio tracks:",
                ["de"] = "Audiospuren:",
                ["fr"] = "Pistes audio :",
                ["es"] = "Pistas de audio:",
                ["it"] = "Tracce audio:",
                ["pt"] = "Faixas de áudio:",
                ["ru"] = "Аудиодорожки:",
                ["pl"] = "Ścieżki audio:",
                ["cs"] = "Zvukové stopy:",
                ["nl"] = "Audiotracks:",
                ["sv"] = "Ljudspår:",
                ["tr"] = "Ses parçaları:"
            },
            ["subTracks"] = new()
            {
                ["hr"] = "Subtitle trackovi:",
                ["en"] = "Subtitle tracks:",
                ["de"] = "Untertitelspuren:",
                ["fr"] = "Pistes de sous-titres :",
                ["es"] = "Pistas de subtítulos:",
                ["it"] = "Tracce sottotitoli:",
                ["pt"] = "Faixas de legenda:",
                ["ru"] = "Дорожки субтитров:",
                ["pl"] = "Ścieżki napisów:",
                ["cs"] = "Stopy titulků:",
                ["nl"] = "Ondertiteltracks:",
                ["sv"] = "Undertextspår:",
                ["tr"] = "Altyazı parçaları:"
            },
            ["aboutLine1"] = new()
            {
                ["hr"] = "Windows video player",
                ["en"] = "Windows video player",
                ["de"] = "Windows-Videoplayer",
                ["fr"] = "Lecteur vidéo Windows",
                ["es"] = "Reproductor de video para Windows",
                ["it"] = "Lettore video per Windows",
                ["pt"] = "Reprodutor de vídeo Windows",
                ["ru"] = "Видеоплеер для Windows",
                ["pl"] = "Odtwarzacz wideo Windows",
                ["cs"] = "Windows přehrávač videa",
                ["nl"] = "Windows-videospeler",
                ["sv"] = "Windows-videospelare",
                ["tr"] = "Windows video oynatıcı"
            },
            ["aboutLine2"] = new()
            {
                ["hr"] = "s punom podrškom za titlove",
                ["en"] = "with full subtitle support",
                ["de"] = "mit voller Untertitelunterstützung",
                ["fr"] = "avec prise en charge complète des sous-titres",
                ["es"] = "con soporte completo de subtítulos",
                ["it"] = "con pieno supporto ai sottotitoli",
                ["pt"] = "com suporte completo a legendas",
                ["ru"] = "с полной поддержкой субтитров",
                ["pl"] = "z pełną obsługą napisów",
                ["cs"] = "s plnou podporou titulků",
                ["nl"] = "met volledige ondertitelondersteuning",
                ["sv"] = "med fullt undertextstöd",
                ["tr"] = "tam altyazı desteğiyle"
            },
            ["aboutDev"] = new()
            {
                ["hr"] = "Razvio: Mako",
                ["en"] = "Developed by: Mako",
                ["de"] = "Entwickelt von: Mako",
                ["fr"] = "Développé par : Mako",
                ["es"] = "Desarrollado por: Mako",
                ["it"] = "Sviluppato da: Mako",
                ["pt"] = "Desenvolvido por: Mako",
                ["ru"] = "Разработал: Mako",
                ["pl"] = "Twórca: Mako",
                ["cs"] = "Vytvořil: Mako",
                ["nl"] = "Ontwikkeld door: Mako",
                ["sv"] = "Utvecklad av: Mako",
                ["tr"] = "Geliştiren: Mako"
            },
            ["close"] = new()
            {
                ["hr"] = "Zatvori",
                ["en"] = "Close",
                ["de"] = "Schließen",
                ["fr"] = "Fermer",
                ["es"] = "Cerrar",
                ["it"] = "Chiudi",
                ["pt"] = "Fechar",
                ["ru"] = "Закрыть",
                ["pl"] = "Zamknij",
                ["cs"] = "Zavřít",
                ["nl"] = "Sluiten",
                ["sv"] = "Stäng",
                ["tr"] = "Kapat"
            },
            ["coffeeFree"] = new()
            {
                ["hr"] = "MakoMiniPlayer je besplatan.",
                ["en"] = "MakoMiniPlayer is free.",
                ["de"] = "MakoMiniPlayer ist kostenlos.",
                ["fr"] = "MakoMiniPlayer est gratuit.",
                ["es"] = "MakoMiniPlayer es gratis.",
                ["it"] = "MakoMiniPlayer è gratuito.",
                ["pt"] = "MakoMiniPlayer é gratuito.",
                ["ru"] = "MakoMiniPlayer бесплатен.",
                ["pl"] = "MakoMiniPlayer jest darmowy.",
                ["cs"] = "MakoMiniPlayer je zdarma.",
                ["nl"] = "MakoMiniPlayer is gratis.",
                ["sv"] = "MakoMiniPlayer är gratis.",
                ["tr"] = "MakoMiniPlayer ücretsizdir."
            },
            ["coffeeMsg"] = new()
            {
                ["hr"] = "Ako vam se sviđa, počastite\nrazvoj jednom kavom!",
                ["en"] = "If you like it, support the\ndevelopment with a coffee!",
                ["de"] = "Wenn es dir gefällt, unterstütze\ndie Entwicklung mit einem Kaffee!",
                ["fr"] = "Si vous l'aimez, soutenez le\ndéveloppement avec un café !",
                ["es"] = "Si te gusta, ¡apoya el\ndesarrollo con un café!",
                ["it"] = "Se ti piace, sostieni lo\nsviluppo con un caffè!",
                ["pt"] = "Se gostar, apoie o\ndesenvolvimento com um café!",
                ["ru"] = "Если нравится, поддержите\nразработку чашкой кофе!",
                ["pl"] = "Jeśli ci się podoba, wesprzyj\nrozwój kawą!",
                ["cs"] = "Pokud se vám líbí, podpořte\nvývoj kávou!",
                ["nl"] = "Als je het leuk vindt, steun de\nontwikkeling met een koffie!",
                ["sv"] = "Om du gillar det, stöd\nutvecklingen med en kaffe!",
                ["tr"] = "Beğendiyseniz, geliştirmeyi\nbir kahveyle destekleyin!"
            },
            ["openPaypal"] = new()
            {
                ["hr"] = "Otvori PayPal",
                ["en"] = "Open PayPal",
                ["de"] = "PayPal öffnen",
                ["fr"] = "Ouvrir PayPal",
                ["es"] = "Abrir PayPal",
                ["it"] = "Apri PayPal",
                ["pt"] = "Abrir PayPal",
                ["ru"] = "Открыть PayPal",
                ["pl"] = "Otwórz PayPal",
                ["cs"] = "Otevřít PayPal",
                ["nl"] = "PayPal openen",
                ["sv"] = "Öppna PayPal",
                ["tr"] = "PayPal'ı aç"
            },
        };

        public static string Get(string key)
        {
            if (T.TryGetValue(key, out var langs))
            {
                if (langs.TryGetValue(CurrentLang, out var val))
                    return val;
                if (langs.TryGetValue("en", out var en))
                    return en;
            }
            return key;
        }
    }
}