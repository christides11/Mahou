using Mahou.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Mahou.Simulation
{
    public class ClientSimulationAdjuster : ISimulationAdjuster
    {
        public float AdjustedInterval { get; private set; } = 1.0f;

        // The actual number of ticks our inputs are arriving ahead of the server simulation.
        // The goal of the adjuster is to get this value as close to 1 as possible without going under.
        //private Ice.MovingAverage actualTickLeadAvg = new Ice.MovingAverage((int)Settings.ServerSendRate * 2);
        private MovingAverage actualTickLeadAvg;

        private int estimatedMissedInputs;

        private Stopwatch droppedInputTimer = new Stopwatch();

        private uint buffer;

        public ClientSimulationAdjuster(int serverSendRate, uint buffer)
        {
            actualTickLeadAvg = new MovingAverage((int)serverSendRate * 2);
            this.buffer = buffer;
        }

        /// <summary>
        ///  When the client joins the server, the tick that it sent to start on is already out of date.
        ///  The client also wants to be ahead of the server by a certain tick amount, so we calculate both here.
        /// </summary>
        /// <param name="receivedServerTick"></param>
        /// <param name="rtt"></param>
        /// <returns></returns>
        public uint DetermineStartTick(uint receivedServerTick, float rtt, float tickTime)
        {
            // By the time we send our inputs to the server, it will already be rtt/2 seconds out of date.
            // We account for this by having the client run ahead of the server by rtt/2 ticks, plus some buffer.
            //uint estimatedTickLead = (uint)((rtt / 2.0f) / tickTime) + buffer;
            uint estimatedTickLead = (uint)(rtt * 1.5 / tickTime) + 4;
            Console.WriteLine($"Tick lead of {estimatedTickLead}, starting at {receivedServerTick + estimatedTickLead} ping of {rtt}. " +
                $"Server tick was {receivedServerTick}");
            return receivedServerTick + estimatedTickLead;
        }

        public void NotifyActualTickLead(int actualTickLead)
        {
            actualTickLeadAvg.ComputeAverage(actualTickLead);

            // TODO: This logic needs significant tuning.

            // Negative lead means dropped inputs which is worse than buffering, so immediately move the
            // simulation forward.
            if (actualTickLead < 0)
            {
                droppedInputTimer.Restart();
                estimatedMissedInputs++;
            }

            var avg = actualTickLeadAvg.Average;
            if (droppedInputTimer.IsRunning && droppedInputTimer.ElapsedMilliseconds < 1000)
            {
                if (avg <= -16)
                {
                    AdjustedInterval = 0.875f;
                }
                else if (avg <= -8)
                {
                    AdjustedInterval = 0.9375f;
                }
                else
                {
                    AdjustedInterval = 0.96875f;
                }
                return;
            }

            // Check for a steady average of a healthy connection before backing off the simulation.
            if (avg >= 16)
            {
                AdjustedInterval = 1.125f;
            }
            else if (avg >= 8)
            {
                AdjustedInterval = 1.0625f;
            }
            else if (avg >= 4)
            {
                AdjustedInterval = 1.03125f;
            }
            //else if (avg >= 2)
            //{
            //    AdjustedInterval = 1.015625f;
            //}
            else
            {
                AdjustedInterval = 1f;
            }
        }
    }
}