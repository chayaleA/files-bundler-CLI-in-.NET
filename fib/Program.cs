//fib bundle --output D:\folder\bundleFile.txt
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics.SymbolStore;
using System.Text;

//bundle command
var createRspCommand = new Command("create-rsp", "response command");
createRspCommand.SetHandler(CreateRspFile);
var bundleCommand = new Command("bundle", "Bundle code files to a single file.");

//options
var outputOption = new Option<FileInfo>(new[] { "--output", "-o" }, "File name and path.");
var languageOption = new Option<string>(new[] { "--language", "-l" }, "List of programming languages, use 'all' to include all code files.");
languageOption.IsRequired = true;
var noteOption = new Option<bool>(new[] { "--note", "-n" }, "Whether to write the source of the code as a comment in the bundle code.");
var sortOption = new Option<string>(new[] { "--sort", "-s" }, "The order of copying the code files. 'name' for alphabetical order of the file's name, 'type' for code type.");
sortOption.SetDefaultValue("name");
var removeEmptyLinesOption = new Option<bool>(new[] { "--remove-empty-lines", "-r" }, "Delete empty lines from the source code before copying to the bundle file.");
var authoroption = new Option<string>(new[] { "--author", "-a" }, "Registering the name of the creator of the file.");

//add the options to the bundle command
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authoroption);

bundleCommand.SetHandler((language, output, note, sort, removeEmptyLines, author) =>
{
    try
    {
        string[] files;

        if (language.ToLower() == "all")
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            files = Directory.GetFiles(currentDirectory);
            files = files.Where(file => !file.Contains("bin") && !file.Contains("debug")).ToArray();
        }
        else
        {
            var desireLanguage = GetDesireLanguage(language);
            var currentDirectory = Directory.GetCurrentDirectory();
            if (desireLanguage != null)
            {
                files = Directory.GetFiles(currentDirectory, $"*.{desireLanguage}");
                files = files.Where(file => !file.Contains("bin") && !file.Contains("debug")).ToArray();
            }
            else
                files = null;
        }

        if (files == null || files.Length == 0)
        {
            Console.WriteLine("No desired files found");
            return;
        }

        if (sort == "type")
            files = files.OrderBy(file => Path.GetExtension(file)).ThenBy(file => Path.GetFileName(file)).ToArray();
        else
            files = files.OrderBy(file => Path.GetFileName(file)).ToArray();

        var bundleContent = new StringBuilder();

        if (!string.IsNullOrEmpty(author))
            bundleContent.AppendLine($"// Author: {author}");

        foreach (var file in files)
        {
            if (note)
            {
                bundleContent.AppendLine($"// Source: {Path.GetFileName(file)} - {file}");
            }

            var fileContents = File.ReadAllText(file);

            if (removeEmptyLines)
            {
                fileContents = RemoveEmptyLines(fileContents);
            }

            bundleContent.AppendLine(fileContents);
        }
        //ישור לשמאל
        var alignedBundleContent = new StringBuilder();
        var lines = bundleContent.ToString().Split('\n');
        foreach (var line in lines)
        {
            alignedBundleContent.AppendLine(line.TrimEnd());
        }

        File.WriteAllText(output.FullName, alignedBundleContent.ToString());
        Console.WriteLine("File was created");
       
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine("Error: File path is invalid");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An unexpected error occurred");
    }
}, languageOption, outputOption, noteOption, sortOption, removeEmptyLinesOption, authoroption);


//register the bundle command to the root
var rootCommand = new RootCommand("Root command for file bundler CLI");
rootCommand.AddCommand(createRspCommand);
rootCommand.Add(bundleCommand);

rootCommand.InvokeAsync(args).Wait();

static void CreateRspFile()
{
    try
    {
        var responseFilePath = "response.rsp";
        var options = new[] { "--language", "--output", "--note", "--sort", "--remove-empty-lines", "--author" };

        var responseFileContent = new StringBuilder();

        foreach (var option in options)
        {
            Console.Write($"Enter desired value for {option}: ");
            var value = Console.ReadLine();
            responseFileContent.AppendLine($"{option}={value}");
        }

        File.WriteAllText(responseFilePath, responseFileContent.ToString());

        Console.WriteLine($"Response file created at {responseFilePath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

static string GetDesireLanguage(string language)
{
    switch (language)
    {
        case "c#":
            return "cs";
        case "c":
            return "c";
        case "asembler":
            return "asm";
        case "pyton":
            return "py";
        default: return null;
    }
}

static string RemoveEmptyLines(string content)
{
    return string.Join("\n", content.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)));
}



//dotnet publish -o publish
