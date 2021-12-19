using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Mayfly.Attributes;

namespace Mayfly.Services
{
	public class HelpService
	{
		private readonly IServiceProvider provider;
		private readonly CommandService commandService;
		private readonly BotConfig config;
		private readonly List<EmbedBuilder> pages = new List<EmbedBuilder>();
		private readonly Dictionary<string, List<EmbedBuilder>> groupPages = new Dictionary<string, List<EmbedBuilder>>();
		private readonly Dictionary<string, Type> availableEnums = new Dictionary<string, Type>();

		private static readonly Dictionary<Type, string> TypeNames = new Dictionary<Type, string>()
		{
			{typeof(int), "int"},
			{typeof(uint), "uint"},
			{typeof(long), "long"},
			{typeof(ulong), "ulong"},
			{typeof(float), "float"},
			{typeof(short), "short"},
			{typeof(ushort), "ushort"},
			{typeof(byte), "byte"},
			{typeof(string), "string"}
		};
		
		public HelpService(IServiceProvider isp, CommandService cs, BotConfig bc)
		{
			this.provider = isp;
			this.commandService = cs;
			this.config = bc;
		}

		/*
		 * Command specific methods
		 */

		public StringBuilder GetCommandHelp(CommandInfo info, StringBuilder builder)
		{
			builder ??= new StringBuilder();
			builder.AppendLine($"// {info.Summary}");

			if (info.Aliases.Count > 1)
			{
				builder.AppendLine("// Aliases: " + string.Join(", ", info.Aliases));
			}

			if (!string.IsNullOrEmpty(info.Remarks))
			{
				string remarks = info.Remarks.Replace("\n", "\n\t");
				builder.AppendLine($"/* Remarks\n\t{remarks}\n*/");
			}

			if (info.Parameters.Count > 0)
			{
				builder.Append(string.IsNullOrEmpty(info.Module.Group) ? $"{config.Prefix}{info.Name} " : $"{config.Prefix}{info.Module.Group} {info.Name} ");
				string[] parameters = new string[info.Parameters.Count];

				for (int i = 0, len = info.Parameters.Count; i < len; i++)
				{
					ParameterInfo parameter = info.Parameters[i];
							
					if (!TypeNames.TryGetValue(parameter.Type, out string t))
					{
						t = parameter.Type.Name;
					}

					if (parameter.IsOptional)
					{
						if (parameter.DefaultValue is string)
						{
							parameters[i] = $"[{t} {parameter.Name} = \"{parameter.DefaultValue}\"]";
						}
						else
						{
							parameters[i] = $"[{t} {parameter.Name} = {parameter.DefaultValue}]";
						}
					}
					else if (parameter.IsMultiple)
					{
						parameters[i] = $"[params {t}[] {parameter.Name}]";
					}
					else
					{
						parameters[i] = $"[{t} {parameter.Name}]";
					}
				}

				builder.AppendLine(string.Join(" ", parameters) + "\n");
			}
			else
			{
				builder.Append(string.IsNullOrEmpty(info.Module.Group) ? $"{config.Prefix}{info.Name}\n" : $"{config.Prefix}{info.Module.Group} {info.Name}\n");
			}

			return builder;
		}

		public string GetCommandHelp(CommandInfo info)
		{
			return GetCommandHelp(info, new StringBuilder()).ToString();
		}

		public string GetResultHelp(SearchResult result)
		{
			StringBuilder builder = new StringBuilder();

			foreach (CommandMatch match in result.Commands.OrderBy(c => c.Command.Name))
			{
				this.GetCommandHelp(match.Command, builder);
			}

			return builder.ToString();
		}
		
		/*
		 * Paged methods
		 */
		
		public void Build()
		{
			StringBuilder builder = new StringBuilder();
			int padSize = commandService.Commands.Aggregate((l, n) => n.Name.Length > l.Name.Length ? n : l).Name.Length + 2;

			Dictionary<string, List<CommandInfo>> filtered = commandService.Commands
				.Where(x => !x.Module.Attributes.OfType<HiddenAttribute>().Any() && !x.Attributes.OfType<HiddenAttribute>().Any())
				.GroupBy(x => x.Name)			// Group commands by their name
				.Select(x => x.First())			// Only select the first command from the group, this removes duplicate entries
				.OrderBy(x => x.Name)			// Order all the commands alphabetically
				.GroupBy(x => x.Module.Group)	// Now group them by their module group
				.OrderBy(x => x.Key)			// Order the groups alphabetically
				.ToDictionary(x => string.IsNullOrEmpty(x.Key) ? "none" : x.Key, x => x.ToList());

			foreach ((string group, List<CommandInfo> commands) in filtered)
			{
				string suffix = group == "none" ? ">[command]" : $">{group} [command]";
				List<EmbedBuilder> groupPage = new List<EmbedBuilder>();

				foreach (CommandInfo info in commands)
				{
					string line = info.Name.PadRight(padSize) + "// " + info.Summary;
					
					if (builder.Length + line.Length >= 2000 - 10 - suffix.Length)
					{
						EmbedBuilder embed = new EmbedBuilder()
						{
							Title = "Help",
							Description = $"{suffix}\n{Format.Code(builder.ToString(), "cs")}\n",
							Color = new Color(0x30CC71)
						};
						
						pages.Add(embed);
						groupPage.Add(embed);
						builder.Clear();
					}
					
					builder.AppendLine(line);
				}
				
				groupPage.Add(new EmbedBuilder()
				{
					Title = "Help",
					Description = $"{suffix}\n{Format.Code(builder.ToString(), "cs")}\n",
					Color = new Color(0x30CC71)
				});

				pages.Add(new EmbedBuilder()
				{
					Title = "Help",
					Description = $"{suffix}\n{Format.Code(builder.ToString(), "cs")}\n",
					Color = new Color(0x30CC71)
				});
				
				groupPages.Add(group, groupPage);
				builder.Clear();
			}

			foreach (CommandInfo command in this.commandService.Commands)
			{
				foreach (ParameterInfo parameter in command.Parameters)
				{
					if (parameter.Type.IsEnum)
					{
						if (!this.availableEnums.ContainsKey(parameter.Type.Name))
						{
							this.availableEnums.Add(parameter.Type.Name, parameter.Type);
						}
					}
				}
			}
		}

		public bool TryGetEnum(string name, out string data)
		{
			if (this.availableEnums.TryGetValue(name, out Type type))
			{
				data = string.Join(", ", Enum.GetNames(type));
				return true;
			}

			data = null;
			return false;
		}

		public IEnumerable<EmbedBuilder> GetPages()
		{
			return this.pages.AsReadOnly();
		}

		public bool TryGetPage(int page, out EmbedBuilder data)
		{
			page--;

			if (page <= this.pages.Count - 1)
			{
				data = this.pages[page];
				return true;
			}

			data = null;
			return false;
		}

		public string GetGroups()
		{
			return string.Join(", ", this.groupPages.Keys);
		}

		public bool TryGetPages(string suffix, out IReadOnlyCollection<EmbedBuilder> builders)
		{
			if (this.groupPages.TryGetValue(suffix, out List<EmbedBuilder> list))
			{
				builders = list.AsReadOnly();
				return true;
			}

			builders = null;
			return false;
		}
	}
}