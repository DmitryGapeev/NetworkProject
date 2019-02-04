using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using static System.Int32;

namespace NetworkProject
{
	class Program
	{
		static void Main(string[] args)
		{
			var network = CreateNetwork();
			//	Console.WriteLine(network.Nodes.Count());
			//	network.DeepTraverse();
			//	Console.WriteLine();
			//	network.WidthTraverse();

			//Console.WriteLine(network.MakeSpanningTree(network.Nodes.FirstOrDefault()));
			//Console.WriteLine(network.MakeMinimalSpanningTreee(network.Nodes.FirstOrDefault()));

			var cost = network.FindAnyPath(network.Nodes[0], network.Nodes[network.Nodes.Count-2], out var pathLinks);
			Console.WriteLine($"path from {network.Nodes[0].Name} to {network.Nodes[network.Nodes.Count - 2].Name} cost = {cost}");

			foreach (var link in pathLinks)
				Console.WriteLine($"{link.From.Name}->{link.To.Name}");

			cost = network.FindLabelSettingsPath(network.Nodes[0], network.Nodes[network.Nodes.Count - 2], out var labelSettingLinks);
			Console.WriteLine($"path from {network.Nodes[0].Name} to {network.Nodes[network.Nodes.Count - 2].Name} cost = {cost}");

			foreach (var link in labelSettingLinks)
				Console.WriteLine($"{link.From.Name}->{link.To.Name}");

			/*cost = network.FindCorrectLabelPath(network.Nodes[0], network.Nodes[network.Nodes.Count - 2], out var labelCorrectPath);
			Console.WriteLine($"path from {network.Nodes[0].Name} to {network.Nodes[network.Nodes.Count - 2].Name} cost = {cost}");

			foreach (var link in labelCorrectPath)
				Console.WriteLine($"{link.From.Name}->{link.To.Name}");*/

			network.FindAllPairsPaths(out var distance, out var via);
			cost = network.FindAllPairsPath(network.Nodes[0], network.Nodes[network.Nodes.Count - 2], distance, via);
			Console.WriteLine($"find all pairs cost = {cost}");

			Console.ReadKey();
		}

		static Network CreateNetwork()
		{
			var rnd = new Random();
			var nodeCount = rnd.Next(30, 50);
			var nodes = new List<Node>();
			var network = new Network();

			var s = 'a';

			for (var i = 0; i < nodeCount; i++, s++)
			{
				var newNode = new Node(s.ToString());
				nodes.Add(newNode);
				network.AddNode(newNode);
			}

			for (var i = 0; i < nodes.Count; i++)
			{
				var countLinks = rnd.Next(1, nodeCount / 3);
				var currentNode = nodes[i];

				for (var j = 0; j < countLinks;)
				{
					var toNodeIndex = rnd.Next(0, nodeCount - 1);
					var toNode = nodes[toNodeIndex];
					var existLink = currentNode.Links.Any(l => l.To == toNode);

					if (i == toNodeIndex)
						continue;

					if (existLink)
					{
						j++;
						continue;
					}

					var cost = rnd.Next(10, 300);
					currentNode.AddLink(toNode, cost);
					toNode.AddLink(currentNode, cost);
					j++;
				}
			}

			return network;
		}
	}

	class Node
	{
		public string Name { get; }
		public List<Link> Links { get; }

		public Node FromNode { get; set; }

		public decimal Distance { get; set; }

		public bool Visited { get; set; }

		public bool FromLink { get; set; }

		public int Index { get; set; }

	public Node(string name)
		{
			Name = name;
			Links = new List<Link>();
			Visited = false;
		}

		public void AddLink(Node to, decimal cost)
		{
			Links.Add(new Link(this, to, cost));
		}
	}

	class Link
	{
		public Node From { get; }
		public Node To { get; }

		public decimal Cost { get; }

		public Link(Node from, Node to, decimal cost)
		{
			From = from;
			To = to;
			Cost = cost;
			Visited = false;
		}

		public bool Visited { get; set; }
	}

	class Network
	{
		public List<Node> Nodes{ get; } = new List<Node>();

		public void AddNode(Node newNode)
		{
			Nodes.Add(newNode);
		}

		private void SetUnvisited()
		{
			foreach (var node in Nodes)
			{
				node.Visited = false;
				node.FromNode = null;
				node.Distance = 0;
				foreach (var link in node.Links)
					link.Visited = false;
			}
		}

		public void WidthTraverse()
		{
			SetUnvisited();
			var rootNode = Nodes[0];
			rootNode.Visited = true;
			var queueNodes = new Queue<Node>();
			queueNodes.Enqueue(rootNode);

			while (queueNodes.Count > 0)
			{
				var currentNode = queueNodes.Dequeue();
				Console.Write(currentNode.Name + " ");

				foreach (var link in currentNode.Links)
				{
					if (!link.To.Visited)
					{
						queueNodes.Enqueue(link.To);
						link.To.Visited = true;
					}
				}
			}
		}

		public void DeepTraverse()
		{
			SetUnvisited();
			var rootNode = Nodes[0];
			rootNode.Visited = true;
			var queueNodes = new Stack<Node>();
			queueNodes.Push(rootNode);

			while (queueNodes.Count > 0)
			{
				var currentNode = queueNodes.Pop();
				Console.Write(currentNode.Name + " ");

				foreach (var link in currentNode.Links)
				{
					if (!link.To.Visited)
					{
						queueNodes.Push(link.To);
						link.To.Visited = true;
					}
				}
			}
		}

		public decimal MakeSpanningTree(Node fromNode)
		{
			SetUnvisited();

			var rootNode = fromNode;
			decimal cost = 0;
			var queueNodes = new Queue<Node>();

			rootNode.Visited = true;
			queueNodes.Enqueue(rootNode);

			while (queueNodes.Count > 0)
			{
				var currentNode = queueNodes.Dequeue();
				//Console.Write(currentNode.Name + " ");

				foreach (var link in currentNode.Links)
				{
					var toNode = link.To;
					if (!toNode.Visited)
					{
						queueNodes.Enqueue(link.To);
						toNode.Visited = true;
						link.Visited = true;
						toNode.FromNode = currentNode;
						cost += link.Cost;
					}
				}
			}

			return cost;
		}

		public decimal MakeMinimalSpanningTreee(Node fromNode)
		{
			SetUnvisited();

			var rootNode = fromNode;
			var candidateLinks = new List<Link>();
			decimal cost = 0;

			rootNode.Visited = true;
			Console.Write(rootNode.Name+" ");

			foreach (var link in rootNode.Links)
				candidateLinks.Add(link);

			while (candidateLinks.Count > 0)
			{
				var minCost = decimal.MaxValue;
				Link minLink = null;

				foreach (var link in candidateLinks)
				{
					if (link.Cost < minCost)
					{
						minLink = link;
						minCost = link.Cost;
					}
				}

				if(minLink == null)
					break;

				candidateLinks.Remove(minLink);

				var toNode = minLink.To;

				if(toNode.Visited)
					continue;
				
				Console.Write(toNode.Name+" ");
				cost += minLink.Cost;
				toNode.Visited = true;

				foreach (var link in toNode.Links)
				{
					if(!link.To.Visited)
						candidateLinks.Add(link);
				}
			}

			return cost;
		}

		private void MakeLabelSettingPath(Node fromNode)
		{
			SetUnvisited();

			var currentNode = fromNode;
			var candidateList = new List<Link>();
			var cost = 0M;
			currentNode.Distance = 0;
			currentNode.Visited = true;

			foreach (var link in fromNode.Links)
				candidateList.Add(link);

			while (candidateList.Count > 0)
			{
				var minDistance = decimal.MaxValue;
				Link minLink = null;

				foreach (var testLink in candidateList)
				{
					var testDistance = testLink.From.Distance + testLink.Cost;

					if (testDistance >= minDistance) continue;

					minDistance = testDistance;
					minLink = testLink;
				}

				if(minLink == null)
					break;

				candidateList.Remove(minLink);

				if(!minLink.To.Visited)
				{
					var toNode = minLink.To;
					toNode.Distance = minDistance;
					toNode.FromNode = minLink.From;
					toNode.Visited = true;
					minLink.Visited = true;
					cost += minDistance;

					foreach (var link in toNode.Links)
					{
						if (!link.To.Visited)
							candidateList.Add(link);
					}
				}
			}
		}

		private decimal MakeCorrectLabelPath(Node fromNode)
		{
			SetUnvisited();
			var candidateList = new List<Link>();
			var cost = 0M;
			fromNode.Distance = 0;
			fromNode.Visited = true;

			foreach (var link in fromNode.Links)
				candidateList.Add(link);

			while (candidateList.Count > 0)
			{
				var minDistance = decimal.MaxValue;
				Link minLink = null;

				foreach (var testLink in candidateList)
				{
					var testDistance = testLink.From.Distance + testLink.Cost;
					if (testDistance < minDistance)
					{
						minDistance = testDistance;
						minLink = testLink;
					}
				}

				if(minLink == null) break;

				candidateList.Remove(minLink);

				var toNode = minLink.To;

				if (!toNode.Visited || toNode.Distance > minDistance)
				{
					toNode.Distance = minDistance;
					toNode.Visited = true;
					toNode.FromNode = minLink.From;
					minLink.Visited = true;

					if (!toNode.Visited)
					{
						foreach (var link in toNode.Links)
							candidateList.Add(link);

						cost += minDistance;
					}
					else
					{
						cost -= toNode.Distance;
						cost += minDistance;
					}
				}
			}

			return cost;

		}

		private decimal FindPathByTree(Node from, Node to, out List<Link> pathLinks)
		{
			pathLinks = new List<Link>();
			var cost = 0M;
			var currentNode = to;

			while (currentNode != from)
			{
				var fromNode = currentNode.FromNode;
				var link = fromNode.Links.FirstOrDefault(l => l.To == currentNode && l.Visited);

				if (link == null) break;

				cost += link.Cost;
				currentNode = currentNode.FromNode;
				pathLinks.Add(link);
			}

			pathLinks.Reverse();

			return cost;
		}

		public decimal FindAnyPath(Node from, Node to, out List<Link> pathLinks)
		{
			MakeSpanningTree(from);

			return FindPathByTree(from, to, out pathLinks);
		}

		public decimal FindLabelSettingsPath(Node from, Node to, out List<Link> patLinks)
		{
			MakeLabelSettingPath(from);
			return FindPathByTree(from, to, out patLinks);
		}

		public decimal FindCorrectLabelPath(Node from, Node to, out List<Link> pathLinks)
		{
			MakeCorrectLabelPath(from);
			return FindPathByTree(from, to, out pathLinks);
		}

		public void FindAllPairsPaths(out decimal[,] distance, out int[,] via)
		{
			const int infinity = MaxValue / 2;
			var count = Nodes.Count;
			distance = new decimal[count, count];
			via = new int[count, count];
			var index = 0;

			foreach (var node in Nodes)
				node.Index = index++;

			for (var i = 0; i < count; i++)
			for (var j = 0; j < count; j++)
			{
				distance[i, j] = infinity;
				via[i, j] = -1;
			}

			foreach (var node in Nodes)
			{
				distance[node.Index, node.Index] = 0;
				via[node.Index, node.Index] = node.Index;

				foreach (var link in node.Links)
				{
					distance[node.Index, link.To.Index] = link.Cost;
					via[node.Index, link.To.Index] = link.To.Index;
				}
			}

			for(var viaIndex = 0; viaIndex < count; viaIndex++)
				for(var fromIndex = 0;fromIndex < count;fromIndex++)
				for (var toIndex = 0; toIndex < count; toIndex++)
				{
					var newDistance = distance[fromIndex, viaIndex] + distance[viaIndex, toIndex];
					if (newDistance < distance[fromIndex, toIndex])
					{
						distance[fromIndex, toIndex] = newDistance;
						via[fromIndex, toIndex] = viaIndex;
					}
				}

		}

		public decimal FindAllPairsPath(Node from, Node to, decimal[,] distance, int[,] via)
		{
			const int infinity = MaxValue / 2;
			if (from.Index == to.Index) return 0;

			if (distance[from.Index, to.Index] == infinity) return -1;

			var viaIndex = via[from.Index, to.Index];

			if (viaIndex == to.Index) return distance[from.Index, to.Index];

			return FindAllPairsPath(Nodes[from.Index], Nodes[viaIndex], distance, via) +
			       FindAllPairsPath(Nodes[viaIndex], Nodes[to.Index], distance, via);
		}
	}
}
