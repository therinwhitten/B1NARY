#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif
namespace B1NARY.Steamworks
{
	using B1NARY.DataPersistence;
	using B1NARY.Scripting;
	using HDConsole;
#if !DISABLESTEAMWORKS
	using global::Steamworks;
#endif
	using Steamworks;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.SocialPlatforms.Impl;
	using static B1NARY.Steamworks.Achievement;

	public class AchievementManager : MonoBehaviour
	{
		public static CommandArray Commands = new()
		{
			["setachievement"] = (Action<string>)((key) =>
			{
				Achievement target = FromKey(key);
#if !DISABLESTEAMWORKS
				if (!target.Exists)
					Debug.Log($"{key} is not real");
#endif
				target.Achieved = true;
			}),
			   //Unlock Command for UnLockables (Non Steam)
			["unlock"] = (Action<string, string, string>)((type, flagName, formalName) =>
			{
				// see Fanart Panel, Saveslot, or Player Config CS for more info
				CollectibleCollection.UnlockUnlockable(type, flagName, formalName);
			}),
		};

		[return: CommandsFromGetter]
		private static HDCommand[] GetHDCommands() => new HDCommand[]
		{
			new HDCommand("bny_achievements", (args) =>
			{
#if DISABLESTEAMWORKS
				StringBuilder builder = new($"<b><size=135%>All {AllAchievements.Count} Achievements:</size></b>\n");
#else
				StringBuilder builder = new($"<b><size=135%>All {AllAchievements.Count} Achievements, with gameID {SteamManager.AppID.m_AppId}:</size></b>\n");
#endif
				for (int i = 0; i < AllAchievements.Count; i++)
				{
					Achievement achievement = AllAchievements[i];
					StringBuilder mainLine = new($"<i>{{{achievement.AchievementIndex}}}</i> <b>{achievement.Name}</b>");
					if (achievement.Achieved == true)
						mainLine.Append(" <size=70%>{Achieved! Nice Job!}</size>");
#if !DISABLESTEAMWORKS
					if (achievement.Exists == false)
						mainLine.Append(" <size=70%><i>And it doesn't exist!...</i></size>");
#endif
					builder.AppendLine(mainLine.ToString());
					if (!string.IsNullOrWhiteSpace(achievement.Description))
						builder.AppendLine($"\t {achievement.Description}");
				}
				HDConsole.WriteLine(builder.ToString());

			}) { description = "Displays all commands into the console." },

			new HDCommand("bny_achivements_reset", (args) =>
			{
				for (int i = 0; i < AllAchievements.Count; i++)
					AllAchievements[i].Achieved = false;

			}) { description = "Resets all achievements to default." },

			new HDCommand("bny_achievement_set", new string[] { "Achievement Index", "0-1" }, (args) =>
			{
				Achievement target = FromKey(args[0]);
				bool enabled = HDCommand.ParseFrom01(args[1]);
				target.Achieved = enabled;
			}) { description = "Sets a specific achievement via key in true or false." },
		};

		public void SetAchievement(string indexKey)
		{
			Achievement target = AllAchievements.First(achievement => achievement.AchievementIndex == indexKey);
			target.Achieved = true;
		}
	}


	public record Achievement(string AchievementIndex, string Name, string Description)
	{
		// Primary Achievements
		private static readonly Achievement
			MaleRoute = new("male_route", "I REALLY want a cup of Coffee...", "Male Route Complete"),
			FemaleRoute = new("female_route", "Jujitsu Training did NOT Pay off...", "Female Route Complete"),
			FemaleRouteHScene = new("demo_female_hscene", "Yuri between the sheets.", "Female H Scene First Time"),
			MaleRouteHScene = new("demo_male_hscene", "Hentai Protag.", "Male H Scene First Time"),
			AllAchievementsCompleted = new("demo_complete", "Cultured", "Demo Complete");

		// Secondary Achievements
		private static readonly Achievement
			NutCracker = new("nut_cracker", "Nut Cracker", "Defend against Lucas"),
			SecondSwallow = new("second_swallow", "Second Swallow", "Make the Male Player say Swallow Twice in Game"),
			InternUWU = new("intern_UwU", "Intern", "First New Game"),
			ClosedBetaDemo = new("closed_beta_demo", "Beta Tester", "Performed Beta Testing for the Demo!");

		// The Best Achievement
		private static readonly Achievement VeryCool = new("very_cool", "Very Cool", "Very Cool");

		public static IReadOnlyList<Achievement> AllAchievements { get; } = new Achievement[]
		{
			MaleRoute,
			FemaleRoute,
			FemaleRouteHScene,
			MaleRouteHScene,
			AllAchievementsCompleted,
			NutCracker,
			SecondSwallow,
			InternUWU,
			ClosedBetaDemo,
		};

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Constructor()
		{
			TryUnlockCompleteDemo();
			for (int i = 0; i < AllAchievements.Count; i++)
				AllAchievements[i].AchievedAchievement += TryUnlockCompleteDemo;
		}
		private static void TryUnlockCompleteDemo(Achievement sourceAchivement = null)
		{
			if (sourceAchivement is not null && ReferenceEquals(sourceAchivement, AllAchievementsCompleted))
				return;
#if !DISABLESTEAMWORKS
			if (!SteamManager.HasInstanceOrInitialize())
				return;
#endif
			if (!MaleRoute.Achieved)
				return;
			if (!FemaleRoute.Achieved)
				return;
			if (!FemaleRouteHScene.Achieved)
				return;
			if (!MaleRouteHScene.Achieved)
				return;
			AllAchievementsCompleted.Achieved = true;
		}
		public static Achievement FromKey(string achievementKey)
		{
			for (int i = 0; i < AllAchievements.Count; i++)
			{
				if (AllAchievements[i].AchievementIndex != achievementKey)
					continue;
				return AllAchievements[i];
			}
			throw new IndexOutOfRangeException(achievementKey);
		}

#if !DISABLESTEAMWORKS
		public bool Exists
		{
			get
			{
				if (m_exists is null)
				{
					m_exists = SteamUserStats.GetAchievement(AchievementIndex, out bool outset);
					m_achieved = outset;
				}
				return m_exists.Value;
			}
		}
		private bool? m_exists = null;
#endif

		public event Action<Achievement> AchievedAchievement;
		public bool Achieved
		{
#if DISABLESTEAMWORKS
			get => PlayerConfig.Instance.savedAchievements.Contains(AchievementIndex);
			set
			{
				if (value)
				{
					PlayerConfig.Instance.savedAchievements.Add(AchievementIndex);
					AchievedAchievement?.Invoke(this);
				}
				else
				{
					PlayerConfig.Instance.savedAchievements.Remove(AchievementIndex);
				}
			}
#else
			get
			{
				if (m_achieved is null)
				{
					m_exists = SteamUserStats.GetAchievement(AchievementIndex, out bool outset);
					m_achieved = outset;
				}
				return m_achieved.Value;
			}
			set
			{
				if (value == m_achieved)
					return;
				m_achieved = value;
				if (!SteamManager.HasInstanceOrInitialize())
					return;

				if (value)
				{
					PlayerConfig.Instance.savedAchievements.Add(AchievementIndex);
					if (Exists)
						SteamUserStats.SetAchievement(AchievementIndex);
					AchievedAchievement?.Invoke(this);
				}
				else
				{
					PlayerConfig.Instance.savedAchievements.Remove(AchievementIndex);
					if (Exists)
						SteamUserStats.ClearAchievement(AchievementIndex);
				}
				SteamUserStats.StoreStats();
			}
#endif
		}
#if !DISABLESTEAMWORKS
		private bool? m_achieved = null;
#endif
	}
}
