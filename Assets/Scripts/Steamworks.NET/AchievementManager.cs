namespace B1NARY.Steamworks
{
	using B1NARY.Scripting;
	using global::Steamworks;
	using Steamworks;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.SocialPlatforms.Impl;
	using static B1NARY.Steamworks.SteamAchievement;

	public class AchievementManager : MonoBehaviour
	{
		public static CommandArray Commands = new()
		{
			["setachievement"] = (Action<string>)((key) =>
			{
				if (!SteamManager.Initialized)
					return;
				AchievementIndex index = Enum.Parse<AchievementIndex>(key);
				SteamAchievement target = AllAchievements.First(achievement => achievement.Index == index);
				if (!target.Exists)
					Debug.Log("s");
				target.Unlock();
			}),
		};

		public void SetAchievement(AchievementIndex index)
		{
			if (!SteamManager.Initialized)
				return;
			SteamAchievement target = AllAchievements.First(achievement => achievement.Index == index);
			target.Unlock();
		}
		public void SetAchievement(string keyIndex)
		{
			if (!SteamManager.Initialized)
				return;
			AchievementIndex index = Enum.Parse<AchievementIndex>(keyIndex);
			SetAchievement(index);
		}
	}


	public record SteamAchievement(AchievementIndex Index, string Name, string Description)
	{
		public enum AchievementIndex : int
		{
			nut_cracker,
			second_swallow,
		}
		public static IReadOnlyList<SteamAchievement> AllAchievements { get; } = new SteamAchievement[]
		{
			new SteamAchievement(AchievementIndex.nut_cracker, "Nut Cracker", ""),
			new SteamAchievement(AchievementIndex.second_swallow, "Second Swallow", ""),
		};


		public bool Exists
		{
			get
			{
				if (m_exists is null)
				{
					m_exists = SteamUserStats.GetAchievement(Index.ToString(), out bool outset);
					m_achieved = outset;
				}
				return m_exists.Value;
			}
		}
		private bool? m_exists = null;
		public bool Achieved 
		{
			get
			{
				if (m_achieved is null)
				{
					m_exists = SteamUserStats.GetAchievement(Index.ToString(), out bool outset);
					m_achieved = outset;
				}
				return m_achieved.Value;
			}
			private set => m_achieved = value; 
		}
		private bool? m_achieved = null;

		public void Unlock()
		{
			if (Achieved)
				return;
			if (!SteamManager.Initialized)
				return;
			Achieved = true;
			SteamUserStats.SetAchievement(Index.ToString());
		}
	}
}
