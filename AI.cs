using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Engine;

namespace U5Designs {
    /// <summary>
    /// All AI routine's must conform to this update() spec.
    /// </summary>
    internal interface Airoutine {
		void update(double time, PlayState playstate, Vector3 playerposn, Enemy me, bool enable3d, List<PhysicsObject> physList);
    }


/**************************************************************************************************************************************** 
 * Ice cream kid
 * */
	enum KidAnim {walk2d=0, walk3d=1, stand2d=2, stand3d=3};

    internal class Kidmoveto : Airoutine{

		public void update(double time, PlayState playstate, Vector3 playerposn, Enemy me, bool enable3d, List<PhysicsObject> physList) {
			//update current animation and flip scale if necessary
			if(enable3d) {
				me.cycleNumber = (int)(me.moving ? KidAnim.walk3d : KidAnim.stand3d);
				if(me.scale.X < 0) {
					me.scale = new Vector3(-me.scale.X, me.scale.Y, me.scale.Z);
				}
			} else { //2D
				me.cycleNumber = (int)(me.moving ? KidAnim.walk2d : KidAnim.stand2d);
				if((me.velocity.X < 0 && me.scale.X < 0) || (me.velocity.X > 0 && me.scale.X > 0)) {
					me.scale = new Vector3(-me.scale.X, me.scale.Y, me.scale.Z);
				}
			}

            me.attackspeed = 1; //delay between each projectile (hack initilization)
            if (!me.frozen) {
                if (VectorUtil.dist(playerposn, me.location) > 90) {
                    Vector3 dir = VectorUtil.getdir(playerposn, me.location);
                    dir.Y = 0.0f;
                    if (!enable3d) {
                        dir.Z = 0.0f;
                    }
                    dir.NormalizeFast();
                    me.velocity.X = dir.X * me.speed;
                    me.velocity.Z = dir.Z * me.speed;
                    //Don't change y - out of our control

                    //Look ahead to see if we are at an edge
                    Vector3 origLoc = me.location;
                    me.location += me.velocity * 0.1f; //move a little bit in this direction (will undo later)
                    if (!(enable3d ? VectorUtil.overGround3dStrict(me, physList) : VectorUtil.overGround2d(me, physList))) {
                        if (enable3d) {
                            //If we're on edge, move along one axis to corner
                            Vector3 origVel = new Vector3(me.velocity);

                            //Check X first
                            me.velocity.X = 0.0f;
                            me.location = origLoc + me.velocity * 0.1f;
                            if (!VectorUtil.overGround3dStrict(me, physList)) {
                                me.velocity.X = origVel.X;
                                me.velocity.Z = 0.0f;
                                me.location = origLoc + me.velocity * 0.1f;
                                if (!VectorUtil.overGround3dStrict(me, physList)) {
                                    //Now we're at corner
                                    me.velocity.X = 0.0f;
                                    me.velocity.Z = 0.0f;
                                }
                            }
                            //Velocity now resulted in something over ground or is zero
                        }
                        else {
                            //Only one option, so just stop
                            me.velocity.X = 0.0f;
                            me.velocity.Z = 0.0f;
                        }
                    }
                    me.location = origLoc; //undo above
                }
                else {
                    me.velocity.X = 0;
                    me.velocity.Z = 0;
                    me.ChangeState(new KidThrowTime());
                }
            }
        }
    }

    internal class KidThrowTime : Airoutine {
		public void update(double time, PlayState playstate, Vector3 playerposn, Enemy me, bool enable3d, List<PhysicsObject> physList) {
			//update current animation
			me.cycleNumber = (int)(enable3d ? KidAnim.stand3d : KidAnim.stand2d);

			//Flip scale if necessary
			if(!enable3d) {
				if((me.location.X < playerposn.X && me.scale.X > 0) || (me.location.X > playerposn.X && me.scale.X < 0)) {
					me.scale = new Vector3(-me.scale.X, me.scale.Y, me.scale.Z);
				}
			} else { // 3D
				if(me.scale.X < 0) {
					me.scale = new Vector3(-me.scale.X, me.scale.Y, me.scale.Z);
				}
			}

            if (!me.frozen) {
                if (VectorUtil.dist(playerposn, me.location) <= 90) {
                    if (me.attackdelayed)
                        me.attacktimer = me.attacktimer + time;
                    if (me.attacktimer > me.attackspeed) {
                        me.attackdelayed = false;
                        me.attacktimer = 0;
                    }
                    if (!me.attackdelayed) {
                        Vector3 dir = VectorUtil.getdir(playerposn, me.location);
                        //throw icecream
                        Vector3 projlocation = me.location;
                        Vector3 direction = VectorUtil.getdir(playerposn, me.location);
                        if (!enable3d) {
                            projlocation.Z += 0.001f; //break the rendering tie between enemy and projectile, or else they flicker
                        }
                        else {
                            direction.Z = 0.0f;
                            direction.NormalizeFast();
                        }
                        Projectile shot = new Projectile(projlocation, direction, false, me.projectile, playstate.player);
                        //Projectile shot = new Projectile(projlocation, direction, new Vector3(12.5f, 12.5f, 12.5f), new Vector3(6.25f, 6.25f, 6.25f), new Vector3(6.25f, 6.25f, 6.25f), true, true, playstate.enable3d, me.damage, 150, false, false, me.projectileSprite);
                        playstate.objList.Add(shot);
                        playstate.renderList.Add(shot);
                        playstate.colisionList.Add(shot);
                        playstate.physList.Add(shot);
                        playstate.combatList.Add(shot);
                        me.attackdelayed = true;
                    }
                }
                else
                    me.ChangeState(new Kidmoveto());
            }
        }
    }

/**************************************************************************************************************************************** 
 * BIRD
 * */
	enum BirdAnim { fly2d=0, fly3d=1, attack2d=2, attack3d=3 };

    internal class Birdmoveto : Airoutine {

		public void update(double time, PlayState playstate, Vector3 playerposn, Enemy me, bool enable3d, List<PhysicsObject> physList) {
			me.cycleNumber = (int)(enable3d ? BirdAnim.fly3d : BirdAnim.fly2d);

			//Flip scale if necessary
			if(enable3d) {
				if(me.scale.X < 0) {
					me.scale = new Vector3(-me.scale.X, me.scale.Y, me.scale.Z);
				}
			} else { //2D
				if((me.velocity.X < 0 && me.scale.X < 0) || (me.velocity.X > 0 && me.scale.X > 0)) {
					me.scale = new Vector3(-me.scale.X, me.scale.Y, me.scale.Z);
				}
			}

            me.attackspeed = 5; // not used in the same way as other attack speeds. This is used to time out the swoop if it gets stuck
            if (!me.frozen) {
                if (dist2d(playerposn, me.location) > 60) {
                    Vector3 dir = VectorUtil.getdir(playerposn, me.location);
                    dir.Y = 0.0f;
                    if (!enable3d) {
                        dir.Z = 0.0f;
                    }
                    dir.NormalizeFast();
                    me.velocity.X = dir.X * me.speed;
                    me.velocity.Z = dir.Z * me.speed;
                }
                else {
                    me.velocity.X = 0;
                    me.velocity.Z = 0;
                    me.PushState(new BirdDive(me.location, playerposn));
                }
            }
        }

        double dist2d(Vector3 v1, Vector3 v2) { // does a dist using only x and z
            Vector2 tmp = new Vector2(v1.X - v2.X, v1.Z - v2.Z);
            return Math.Sqrt((tmp.X * tmp.X) + (tmp.Y * tmp.Y));
        }

    }

    internal class BirdDive : Airoutine {
        bool diving, finished;
        Vector3 startingposn, startplayer, dir;

        public BirdDive(Vector3 start, Vector3 player) {
            startingposn = start;
            startplayer = player;
            diving = true;
            finished = false;
            dir = VectorUtil.getdir(player, start);
        }

        public void update(double time, PlayState playstate, Vector3 playerposn, Enemy me, bool enable3d, List<PhysicsObject> physList) {
            if (!me.frozen) {
                if (!finished) {
                    if (diving) {
						me.cycleNumber = (int)(enable3d ? BirdAnim.attack3d : BirdAnim.attack2d);

						//Flip scale if necessary
						//TODO: These are specific to 2D, add 3D case
						if((me.velocity.X < 0 && me.scale.X < 0) || (me.velocity.X > 0 && me.scale.X > 0)) {
							me.scale = new Vector3(-me.scale.X, me.scale.Y, me.scale.Z);
						}

                        me.attacktimer = me.attacktimer + time;
                        // do a fake gravity to simulate diving down
                        me.accel.Y -= (float)(200 * time);
                        dir.Y = 0.0f;
                        if (!enable3d) {
                            dir.Z = 0.0f;
                        }
                        dir.NormalizeFast();
                        me.velocity.X = dir.X * (me.speed);
                        me.velocity.Z = dir.Z * (me.speed);
                        // if dove to lowest point, or to much time has passed(we got suck), stop diving
                        if ((Math.Abs(me.location.Y - playerposn.Y) < 10) || me.attacktimer > me.attackspeed) {
                            me.velocity.Y = 0;
                            me.attacktimer = 0;
                            diving = false;
                        }
                    } else {
						me.cycleNumber = (int)(enable3d ? BirdAnim.fly3d : BirdAnim.fly2d);

						//Flip scale if necessary
						if(enable3d) {
							if(me.scale.X < 0) {
								me.scale = new Vector3(-me.scale.X, me.scale.Y, me.scale.Z);
							}
						} else { //2D
							if((me.velocity.X < 0 && me.scale.X < 0) || (me.velocity.X > 0 && me.scale.X > 0)) {
								me.scale = new Vector3(-me.scale.X, me.scale.Y, me.scale.Z);
							}
						}

                        me.attacktimer = me.attacktimer + time;
                        // do a reverse gravity to simulate flying back up
                        me.accel.Y += (float)(200 * time);
                        dir.Y = 0.0f;
                        if (!enable3d) {
                            dir.Z = 0.0f;
                        }
                        dir.NormalizeFast();
                        me.velocity.X = dir.X * (me.speed);
                        me.velocity.Z = dir.Z * (me.speed);
                        //if back to original height, or to much time has passed(we got suck), just finish
                        if ((me.location.Y >= startingposn.Y) || me.attacktimer > me.attackspeed)
                            finished = true;
                    }
                }
                else {
                    me.attacktimer = 0;
                    me.velocity.X = 0;
                    me.velocity.Y = 0;
                    me.velocity.Z = 0;
                    me.PopState();
                }

            }
        }
    }


/**************************************************************************************************************************************** 
 * Ice cream kid
 * */
    internal class Girlmoveto : Airoutine {
        public void update(double time, PlayState playstate, Vector3 playerposn, Enemy me, bool enable3d, List<PhysicsObject> physList) {
            me.attackspeed = 1; //delay between each projectile (hack initilization)
            if (!me.frozen) {
                Vector3 dir = VectorUtil.getdir(playerposn, me.location);
                dir.Y = 0.0f;
                if (!enable3d) {
                    dir.Z = 0.0f;
                }
                dir.NormalizeFast();
                me.velocity.X = dir.X * me.speed;
                me.velocity.Z = dir.Z * me.speed;
                //Don't change y - out of our control

                //Look ahead to see if we are at an edge
                Vector3 origLoc = me.location;
                me.location += me.velocity * 0.1f; //move a little bit in this direction (will undo later)
                if (!(enable3d ? VectorUtil.overGround3dStrict(me, physList) : VectorUtil.overGround2d(me, physList))) {
                    if (enable3d) {
                        //If we're on edge, move along one axis to corner
                        Vector3 origVel = new Vector3(me.velocity);

                        //Check X first
                        me.velocity.X = 0.0f;
                        me.location = origLoc + me.velocity * 0.1f;
                        if (!VectorUtil.overGround3dStrict(me, physList)) {
                            me.velocity.X = origVel.X;
                            me.velocity.Z = 0.0f;
                            me.location = origLoc + me.velocity * 0.1f;
                            if (!VectorUtil.overGround3dStrict(me, physList)) {
                                //Now we're at corner
                                me.velocity.X = 0.0f;
                                me.velocity.Z = 0.0f;
                            }
                        }
                        //Velocity now resulted in something over ground or is zero
                    }
                    else {
                        //Only one option, so just stop
                        me.velocity.X = 0.0f;
                        me.velocity.Z = 0.0f;
                    }
                }
                me.location = origLoc; //undo above
            }
        }
    }
}
