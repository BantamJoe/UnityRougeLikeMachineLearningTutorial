using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace Completed {


    public class RougeLikeAgent : Agent {

        Vector3[] FindClosestTargetPoints(string tag) {
            Vector3 position = transform.position;
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 40f);

            var closests = hitColliders
                .Where(o => o.gameObject.tag == tag)
                .OrderBy(o => Vector3.Distance(o.ClosestPoint(position), position))
                .Take(3);

            Vector3[] closest_points = new Vector3[3];

            int i = 0;
            foreach (Collider col in closests) {
                closest_points[i] = col.ClosestPoint(position);
                i++;
            }

            return closest_points;
        }

     
        Player _player;
        int[] state = new int[66];
	    public override List<float> CollectState()
	    {
		    List<float> state = new List<float>();

            state.Add(transform.position.x);
            state.Add(transform.position.y);
            state.Add(transform.position.z);

            state.Add(_player.food);

            state.Add(Vector3.Distance(transform.position, new Vector3(7f,7f, transform.position.z)));

            Vector3[] closest_points = FindClosestTargetPoints("Enemies");

            //3 points
            foreach (Vector3 point in closest_points) {
                state.Add(point.x);
                state.Add(point.y);
            }

            closest_points = FindClosestTargetPoints("Soda");
            //3 points
            foreach (Vector3 point in closest_points) {
                state.Add(point.x);
                state.Add(point.y);
            }

            closest_points = FindClosestTargetPoints("Food");
            //3 points
            foreach (Vector3 point in closest_points) {
                state.Add(point.x);
                state.Add(point.y);
            }



            return state;
	    }


        public override void InitializeAgent() {
            brain = GameObject.Find("RougeLikeBrain").GetComponent<Brain>();
            _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            
        }

        int last_food = 100;

        public override void AgentStep(float[] act)
	    {

            int action = (int)act[0];
            switch (action) {
                case 0: //left
                    _player.horizontal = -1;
                    break;
                case 1: //right
                    _player.horizontal = 1;
                    break;
                case 2: //down
                    _player.vertical = -1;
                    break;
                case 3: //up
                    _player.vertical = 1;
                    break;
            }

            if (transform.position.x == 7 && transform.position.y == 7 && !_player.resolved) {
                _player.resolved = true;
                reward = _player.food;
            }

            reward += (_player.food - last_food) / 100f;
            last_food = _player.food;

            if(_player.food <= 1 && !_player.resolved) {
                _player.resolved = true;
                reward = -1;
                done = true;
            }

        }

	    public override void AgentReset()
	    {


        }

	    public override void AgentOnDone()
	    {

        }
    }

}