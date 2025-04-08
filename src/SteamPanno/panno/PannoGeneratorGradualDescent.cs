using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteamPanno.panno
{
	public class PannoGeneratorGradualDescent : PannoGenerator
	{
		private float totalHours;
		private int totalArea;
		private float fixedHours;
		private int fixedArea;
		private float deltaHours;
		private int depthMax;

		public override async Task<PannoNode> Generate(PannoGame[] games, Rect2I area)
		{
			var gamesQueue = new Queue<PannoGame>(games.OrderByDescending(x => x.HoursOnRecord).ToArray());
			totalHours = gamesQueue.Sum(x => x.HoursOnRecord);
			totalArea = area.Size.X * area.Size.Y;
			fixedHours = 0;
			fixedArea = 0;
			deltaHours = 0;
			depthMax = 1;

			return await GenerateInner(gamesQueue, area, 1);
		}

		private async Task<PannoNode> GenerateInner(Queue<PannoGame> games, Rect2I area, int depth)
		{
			depthMax = Math.Max(depthMax, depth);

			while (games.Count > 0)
			{
				var game = games.Peek();
				var areaArea = area.Size.X * area.Size.Y;
				var areaAreaNext = areaArea / 2;
				var areaHours = ((float)areaArea / totalArea) * totalHours;

				if ((games.Count == 1 ||
					areaHours <= game.HoursOnRecord + deltaHours ||
					games.Count * areaArea / 2 < totalArea - fixedArea ||
					(area.PreferHorizontal() ? area.Size.X : area.Size.Y) < 8)
						&& depth == depthMax)
				{
					games.Dequeue();
					fixedHours += game.HoursOnRecord;
					fixedArea += areaArea;
					deltaHours += game.HoursOnRecord - areaHours;

					return new PannoNodeLeaf()
					{
						Game = game,
						Area = area,
					};
				}
				
				var areaFirst = GetFirstArea(area);
				var areaSecond = GetSecondArea(area);
				var nodeFirst = await GenerateInner(games, areaFirst, depth + 1);
				var nodeSecond = await GenerateInner(games, areaSecond, depth + 1);
				return new PannoNodeRoot(nodeFirst, nodeSecond);
			}

			return null;
		}
	}
}
