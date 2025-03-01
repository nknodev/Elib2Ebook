using System.Collections.Generic;
using CommandLine;

namespace Elib2Ebook.Configs; 

public class Options {
    [Option("url", Required = true, HelpText = "Ссылка на книгу", Separator = ',')]
    public IEnumerable<string> Url { get; set; }
        
    [Option("proxy", Required = false, HelpText = "Прокси в формате <host>:<port>", Default = "")]
    public string Proxy { get; set; }
        
    [Option("save", Required = false, HelpText = "Директория для сохранения книги")]
    public string SavePath { get; set; }
        
    [Option("format", Required = true, HelpText = "Формат для сохранения книги", Separator = ',')]
    public IEnumerable<string> Format { get; set; }
    
    [Option("cover", Required = false, HelpText = "Сохранить обложку книги в отдельный файл")]
    public bool Cover { get; set; }

    [Option("login", Required = false, HelpText = "Логин от системы")]
    public string Login { get; set; }
        
    [Option("password", Required = false, HelpText = "Пароль от системы")]
    public string Password { get; set; }
}