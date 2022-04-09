using Mayfly.Structures;
using Newtonsoft.Json;

namespace Mayfly.Services
{
	public enum RexLanguage
	{
		None = 0,
		CSharp = 1,
		VBNET = 2,
		FSharp = 3,
		Java = 4,
		Python = 5,
		C_GCC = 6,
		CPP_GCC = 7,
		Php = 8,
		Pascal = 9,
		ObjectiveC = 10,
		Haskell = 11,
		Ruby = 12,
		Perl = 13,
		Lua = 14,
		Nasm = 15, //*
		SqlServer = 16,
		Javascript = 17, //*
		Lisp = 18,
		Prolog = 19,
		Go = 20,
		Scala = 21,
		Scheme = 22,
		NodeJS = 23,
		Python3 = 24,
		Octave = 25,
		C_Clang = 26,
		CPP_Clang = 27,
		CPP_VC = 28,
		C_VC = 29,
		D = 30,
		R = 31,
		Tcl = 32,
		MySQL = 33, //*
		PostgreSQL = 34,
		Oracle = 35, //*
		Swift = 37,
		Bash = 38,
		Ada = 39,
		Erlang = 40,
		Elixir = 41,
		Ocaml = 42,
		Kotlin = 43,
		Brainfuck = 44,
		Fortran = 45,
		Rust = 46,
		Clojure = 47
	}

	public class RexTesterService
	{
		private readonly HttpService http;

		public RexTesterService(HttpService hs)
		{
			http = hs;
		}
		
		public static RexLanguage GetLanguageFromString(string lang) => lang switch
		{
			"csharp"     => RexLanguage.CSharp,
			"cs"         => RexLanguage.CSharp,
			"vb"         => RexLanguage.VBNET,
			"fsharp"     => RexLanguage.FSharp,
			"java"       => RexLanguage.Java,
			"python"     => RexLanguage.Python3,
			"py"         => RexLanguage.Python3,
			"c"          => RexLanguage.C_Clang,
			"cpp"        => RexLanguage.CPP_Clang,
			"c++"        => RexLanguage.CPP_Clang,
			"php"        => RexLanguage.Php,
			"pascal"     => RexLanguage.Pascal,
			"haskell"    => RexLanguage.Haskell,
			"ruby"       => RexLanguage.Ruby,
			"rb"         => RexLanguage.Ruby,
			"perl"       => RexLanguage.Perl,
			"lua"        => RexLanguage.Lua,
			"sql"        => RexLanguage.SqlServer,
			"javascript" => RexLanguage.NodeJS,
			"js"         => RexLanguage.NodeJS,
			"lisp"       => RexLanguage.Lisp,
			"prolog"     => RexLanguage.Prolog,
			"go"         => RexLanguage.Go,
			"scala"      => RexLanguage.Scala,
			"scheme"     => RexLanguage.Scheme,
			"octave"     => RexLanguage.Octave,
			"d"          => RexLanguage.D,
			"r"          => RexLanguage.R,
			"tcl"        => RexLanguage.Tcl,
			"postgresql" => RexLanguage.PostgreSQL,
			"postgres"   => RexLanguage.PostgreSQL,
			"swift"      => RexLanguage.Swift,
			"bash"       => RexLanguage.Bash,
			"sh"         => RexLanguage.Bash,
			"ada"        => RexLanguage.Ada,
			"elixir"     => RexLanguage.Elixir,
			"ocaml"      => RexLanguage.Ocaml,
			"kotlin"     => RexLanguage.Kotlin,
			"brainfuck"  => RexLanguage.Brainfuck,
			"fortran"    => RexLanguage.Fortran,
			"rust"       => RexLanguage.Rust,
			"rs"         => RexLanguage.Rust,
			"clojure"    => RexLanguage.Clojure,
			"clj"        => RexLanguage.Clojure,
			"asm"        => RexLanguage.Nasm,
			_            => RexLanguage.None
		};

		public static string GetDefaultCompileArgs(RexLanguage language) => language switch
		{
			RexLanguage.CPP_Clang => "-o a.out source_file.cpp",
			RexLanguage.C_Clang => "-o a.out source_file.c",
			RexLanguage.D => "-ofa.out source_file.d",
			RexLanguage.Go => "-o a.out source_file.go",
			RexLanguage.Haskell => "-o a.out source_file.hs",
			_ => ""
		};

		public async Task<RexTester> Compile(RexLanguage language, string program, string input = "", string args = "")
		{
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://rextester.com/rundotnet/api")
			{
				Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[4]
				{
					new KeyValuePair<string, string>("LanguageChoice", language.ToString("D")),
					new KeyValuePair<string, string>("Program", program),
					new KeyValuePair<string, string>("Input", input),
					new KeyValuePair<string, string>("CompilerArgs", args)
				})
			};

			HttpResponseMessage response = await http.SendAsync(request);
			
			if (response.IsSuccessStatusCode)
			{
				try
				{
					return JsonConvert.DeserializeObject<RexTester>(await response.Content.ReadAsStringAsync());
				}
				catch { /* ignore */ }
			}
			
			return null;
		}
	}
}