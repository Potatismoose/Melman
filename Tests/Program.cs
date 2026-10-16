using System;
using System.Collections.Generic;
using Melman;
class Tests {
 static int checks;
 static void Assert(bool ok,string message){checks++;if(!ok)throw new Exception(message);}
 static Cell DirectionTo(Game g,Cell target){var q=new Queue<Cell>();var first=new Dictionary<Cell,Cell>();q.Enqueue(g.Player);first[g.Player]=new Cell();while(q.Count>0){var p=q.Dequeue();foreach(var d in Game.Directions){var n=p+d;if(!g.Walkable(n)||first.ContainsKey(n))continue;first[n]=p.Equals(g.Player)?d:first[p];if(n.Equals(target))return first[n];q.Enqueue(n);}}throw new Exception("Unreachable collectible");}
 static Game Corridor(){
  var g=new Game(Difficulty.Normal);g.Walls.Clear();g.Dots.Clear();g.Powers.Clear();g.Ghosts.Clear();
  for(int y=0;y<Game.Size;y++)for(int x=0;x<Game.Size;x++)g.Walls.Add(new Cell(x,y));
  foreach(var c in new[]{new Cell(1,1),new Cell(2,1),new Cell(3,1),new Cell(3,2),new Cell(3,3)}){g.Walls.Remove(c);if(!c.Equals(g.Player))g.Dots.Add(c);}
  return g;
 }
 static void MotionTests(){
  var g=Corridor();var m=new Movement(g);m.Advance(.03f);Assert(g.Player.Equals(new Cell(1,1)),"Moved without input");
  m.Request(new Cell(1,0));m.Advance(.04f);Assert(g.Player.Equals(new Cell(2,1)),"First press did not start immediately");
  Assert(m.PlayerFrom.Equals(new Cell(1,1))&&m.Fraction>0&&m.Fraction<1,"No intermediate render position");
  m.Request(new Cell(0,1));m.Advance(g.TickSeconds-.04f+.001f);
  Assert(g.Player.Equals(new Cell(3,1)),"Blocked queued turn stopped forward movement");
  Assert(m.Fraction<.02f,"Tick discarded leftover time");
  m.Advance(g.TickSeconds);Assert(g.Player.Equals(new Cell(3,2)),"Released turn request was lost");
  m.Request(new Cell(0,-1));m.Advance(g.TickSeconds);Assert(g.Player.Equals(new Cell(3,1)),"Reverse request ignored");
  var a=Corridor();var b=Corridor();var ma=new Movement(a);var mb=new Movement(b);ma.Request(new Cell(1,0));mb.Request(new Cell(1,0));ma.Advance(.93f);for(int i=0;i<93;i++)mb.Advance(.01f);
  Assert(a.Player.Equals(b.Player)&&Math.Abs(ma.Fraction-mb.Fraction)<.001f,"Frame-rate-dependent movement");
  Assert(a.Player.Equals(new Cell(3,1)),"Movement crossed a wall");
  var death=new Game(Difficulty.Hard);death.Ghosts.Clear();death.Ghosts.Add(new Cell(2,1));death.Walls.Remove(new Cell(2,1));var md=new Movement(death);md.Request(new Cell(1,0));md.Advance(.01f);
  Assert(death.Lives==2&&md.PlayerFrom.Equals(death.Player),"Respawn interpolates through walls");
  md.Advance(.5f);Assert(death.Player.Equals(new Cell(1,1)),"Stale input moved player after death");
 }
 static void Main(){
  MotionTests();
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
  Console.WriteLine("PASS: "+checks+" assertions; 90 seeded mazes, all five levels, reachability, scoring, power, collisions, difficulty, terminal states, buffered turns, interpolation and frame timing.");
 }
}
