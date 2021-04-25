using Mahou.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Mahou.Simulation
{
    /// <summary>
    /// Adjust the time between ticks depending on how ahead or behind the client
    /// is compared to the server. 
    /// </summary>
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
        /// <param name="rtt">The round trip time in seconds.</param>
        /// <param name="tickTime">The time that each tick takes.</param>
        /// <returns></returns>
        public uint DetermineStartTick(uint receivedServerTick, float rtt, float tickTime)
        {
            // By the time we send our inputs to the server, it will already be rtt/2 seconds out of date.
            // We account for this by having the client run ahead of the server by rtt/2 ticks, plus some buffer.
            uint estimatedTickLead = (uint)((rtt / 2.0f) / tickTime) + buffer;
            //uint estimatedTickLead = (uint)(rtt * 1.5 / tickTime) + buffer;
            Console.WriteLine($"Tick lead of {estimatedTickLead}, starting at {receivedServerTick + estimatedTickLead} ping of {rtt}. " +
                $"Server tick was {receivedServerTick}");
            return receivedServerTick + estimatedTickLead;
        }

        public void NotifyActualTickLead(int actualTickLead)
        {
            actualTickLeadAvg.ComputeAverage((decimal)actualTickLead);
            decimal avg = actualTickLeadAvg.Average;

            if(actualTickLead < 0)
            {
                droppedInputTimer.Restart();
                estimatedMissedInputs++;
            }

            float simRate = 1.0f / 60.0f;
            if (droppedInputTimer.IsRunning && droppedInputTimer.ElapsedMilliseconds < 1000)
            {
                // We are behind the server. Use larger values here as dropped inputs is worse than buffering.
                if (avg <= -16)
                {
                    AdjustedInterval = (simRate - (0.10f / 60.0f)) / simRate;
                }
                else if (avg <= -8)
                {
                    AdjustedInterval = (simRate - (0.04f / 60.0f)) / simRate;
                }
                else
                {
                    AdjustedInterval = (simRate - (0.01f / 60.0f)) / simRate;
                }
            } else if (avg > buffer)
            {
                // We are too far ahead.
                if(avg >= (buffer+5))
                {
                    AdjustedInterval = (simRate + (0.06f / 60.0f)) / simRate;
                }
                else if(avg >= (buffer+3))
                {
                    AdjustedInterval = (simRate + (0.03f / 60.0f)) / simRate;
                }
                else
                {
                    AdjustedInterval = (simRate + (0.02f / 60.0f)) / simRate;
                }
            } else
            {
                // We are where we should be, do nothing.
                AdjustedInterval = 1f;
            }
        }
    }
}