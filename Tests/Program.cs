using System;
using System.Collections.Generic;
using Melman;
class Tests {
 static int checks;
 static void Assert(bool ok,string message){checks++;if(!ok)throw new Exception(message);}
 static Cell DirectionTo(Game g,Cell target){var q=new Queue<Cell>();var first=new Dictionary<Cell,Cell>();q.Enqueue(g.Player);first[g.Player]=new Cell();while(q.Count>0){var p=q.Dequeue();foreach(var d in Game.Directions){var n=p+d;if(!g.Walkable(n)||first.ContainsKey(n))continue;first[n]=p.Equals(g.Player)?d:first[p];if(n.Equals(target))return first[n];q.Enqueue(n);}}throw new Exception("Unreachable collectible");}
 static void Main(){
  foreach(Difficulty difficulty in Enum.GetValues(typeof(Difficulty)))for(int seed=0;seed<30;seed++){
   var g=new Game(difficulty,seed);Assert(g.Lives==3&&g.Level==1,"Initial state");
   Assert(g.Ghosts.Count==(difficulty==Difficulty.Easy?2:3),"Difficulty ghost count");
   var seen=new HashSet<Cell>();var q=new Queue<Cell>();q.Enqueue(g.Player);seen.Add(g.Player);while(q.Count>0){var p=q.Dequeue();foreach(var d in Game.Directions)if(g.Walkable(p+d)&&seen.Add(p+d))q.Enqueue(p+d);}
   foreach(var c in g.Dots)Assert(seen.Contains(c),"Disconnected dot");foreach(var c in g.Powers)Assert(seen.Contains(c),"Disconnected power");foreach(var c in g.Ghosts)Assert(seen.Contains(c),"Ghost in wall");
   g.Step(new Cell(-1,0));Assert(g.Player.Equals(new Cell(1,1)),"Walked through wall");
   // Isolate collection/progression from AI, then traverse the actual generated maze.
   for(int level=1;level<=Game.LevelCount;level++){
    g.Ghosts.Clear();int guard=15000;
    while(g.Phase==Phase.Playing&&--guard>0){Cell target=new Cell();foreach(var c in g.Dots){target=c;break;}if(g.Dots.Count==0)foreach(var c in g.Powers){target=c;break;}g.Step(DirectionTo(g,target));}
    Assert(guard>0,"Progression timed out");Assert(g.Dots.Count==0&&g.Powers.Count==0,"Uncollected items");Assert(g.Phase==(level==Game.LevelCount?Phase.Won:Phase.LevelComplete),"Wrong completion state");
    if(level<Game.LevelCount){g.NextLevel();Assert(g.Level==level+1,"Level did not advance");}
   }
   Assert(g.Score>0,"No score");int score=g.Score;g.Step(new Cell(1,0));Assert(g.Score==score,"Game advanced after victory");
  }
  var collision=new Game(Difficulty.Hard);collision.Ghosts.Clear();collision.Ghosts.Add(collision.Player);collision.Step(new Cell());Assert(collision.Lives==2,"Collision life loss");
  while(collision.Phase==Phase.Playing){collision.Ghosts.Clear();collision.Ghosts.Add(collision.Player);collision.Step(new Cell());}Assert(collision.Phase==Phase.GameOver&&collision.Lives==0,"Game over");
  var power=new Game(Difficulty.Normal);power.Ghosts.Clear();while(power.PowerTicks==0)power.Step(DirectionTo(power,new Cell(17,1)));int before=power.Score;power.Ghosts.Add(power.Player);power.Step(new Cell());Assert(power.Lives==3&&power.Score==before+200,"Powered ghost collision");
  Assert(new Game(Difficulty.Easy).TickSeconds>new Game(Difficulty.Hard).TickSeconds,"Difficulty speed");
  Console.WriteLine("PASS: "+checks+" assertions; 90 seeded mazes, all five levels, reachability, scoring, power, collisions, difficulty and terminal states.");
 }
}
