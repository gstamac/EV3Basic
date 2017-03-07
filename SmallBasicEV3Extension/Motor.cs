﻿/*  EV3-Basic: A basic compiler to target the Lego EV3 brick
    Copyright (C) 2015 Reinhard Grafl

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.SmallBasic.Library;
using EV3Communication;

namespace SmallBasicEV3Extension
{

    /// <summary>
    /// Control the Motors connected to the Brick.
    /// For every Motor function you need to specify one or more motor ports that should be affected (for example, "A", "BC", "ABD").
    /// When additional bricks are daisy-chained to the master brick, address the correct port by adding the layer number to the specifier (for example, "3BC", "2A"). In this case only the motors of one brick can be accessed with a single command. 
    /// Speed vs. Power: When requesting to drive a motor with a certain speed, the electrical power will be permanently adjusted to keep the motor on this speed regardless of the necessary driving force (as long as enough power can be provided). When requesting a certain power instead, the motor will be provided with just this much electrical power and the actual speed will then depend on the resistance it meets.
    /// </summary>
    [SmallBasicType]
    public static class Motor
    {

        /// <summary>
        /// Stop one or multiple motors. This will also cancel any scheduled movement for this motor.
        /// </summary>
        /// <param name="ports">Motor port name(s)</param>
        /// <param name="brake">"True", if the motor should use the brake.</param>
        public static void Stop(Primitive ports, Primitive brake)
        {
            int layer;
            int nos;
            DecodePortsDescriptor(ports == null ? "" : ports.ToString(), out layer, out nos);
            int brk = (brake == null ? "" : brake.ToString()).Equals("true", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

            ByteCodeBuffer c = new ByteCodeBuffer();
            c.OP(0xA3);
            c.CONST(layer);
            c.CONST(nos);
            c.CONST(brk);
            EV3RemoteControler.DirectCommand(c, 0, 0);
        }

        /// <summary>
        /// Start one or more motors with the requested speed or set an already running motor to this speed.
        /// </summary>
        /// <param name="ports">Motor port name(s)</param>
        /// <param name="speed">Speed value from -100 (full reverse) to 100 (full forward).</param>
        public static void Start(Primitive ports, Primitive speed)
        {
            int layer;
            int nos;
            DecodePortsDescriptor(ports == null ? "" : ports.ToString(), out layer, out nos);
            int spd = clamp(speed, -100, 100);

            ByteCodeBuffer c = new ByteCodeBuffer();
            c.OP(0xAF);       // opOutput_Time_Speed
            c.CONST(layer);
            c.CONST(nos);
            c.CONST(spd);
            c.CONST(0);           // step1
            c.CONST(2147483647);  // step2 (run over three weeks)
            c.CONST(0);           // step3
            c.CONST(0);           // don't brake
            EV3RemoteControler.DirectCommand(c, 0, 0);
        }

        /// <summary>
        /// Start one or more motors with the requested power or set an already running motor to this power.
        /// </summary>
        /// <param name="ports">Motor port name(s)</param>
        /// <param name="power">Power value from -100 (full reverse) to 100 (full forward).</param>
        public static void StartPower(Primitive ports, Primitive power)
        {
            int layer;
            int nos;
            DecodePortsDescriptor(ports == null ? "" : ports.ToString(), out layer, out nos);
            int pwr = clamp(power, -100, 100);

            ByteCodeBuffer c = new ByteCodeBuffer();
            c.OP(0xAD);       // opOutput_Time_Power
            c.CONST(layer);
            c.CONST(nos);
            c.CONST(pwr);
            c.CONST(0);           // step1
            c.CONST(2147483647);  // step2 (run over three weeks)
            c.CONST(0);           // step3
            c.CONST(0);           // don't brake
            EV3RemoteControler.DirectCommand(c, 0, 0);
        }

        /// <summary>
        /// Set two motors to run synchronized at their chosen speed levels. 
        /// The two motors will be synchronized which means that when one motor experiences some resistance and cannot keep up its speed, the other motor will also slow down or stop altogether. This is especially useful for vehicles with two independently driven wheels which still need to go straight or make a specified turn.
        /// The motors will keep running until stopped by another command.
        /// </summary>
        /// <param name="ports">Name of two motor ports (for example "AB" or "CD").</param>
        /// <param name="speed1">Speed value from -100 (full reverse) to 100 (full forward) of the motor with the lower port letter.</param>
        /// <param name="speed2">Speed value from -100 (full reverse) to 100 (full forward) of the motor with the higher port letter.</param>
        public static void StartSync(Primitive ports, Primitive speed1, Primitive speed2)
        {
            double spd1 = fclamp(speed1, -100, 100);
            double spd2 = fclamp(speed2, -100, 100);
            double speed;
            double turn;

            // motor with lower letter is faster or equally fast and must become master      
            if ((spd1 >= 0 ? spd1 : -spd1) >= (spd2 >= 0 ? spd2 : -spd2))
            {
                speed = spd1;
                turn = 100 - ((100.0 * spd2) / spd1);
            }
            // motor with higher letter is faster and must become master
            else
            {
                speed = spd2;
                turn = -(100 - ((100.0 * spd1) / spd2));
            }

            StartSyncTurn(ports, speed, turn);
        }

        /// <summary>
        /// Set two motors to run synchronized at their chosen speed levels and turn in desired direction. 
        /// The two motors will be synchronized which means that when one motor experiences some resistance and cannot keep up its speed, the other motor will also slow down or stop altogether. This is especially useful for vehicles with two independently driven wheels which still need to go straight or make a specified turn.
        /// The motors will keep running until stopped by another command.
        /// </summary>
        /// <param name="ports">Name of two motor ports (for example "AB" or "CD").</param>
        /// <param name="speed">Speed value from -100 (full reverse) to 100 (full forward) of the motors.</param>
        /// <param name="turn">Turn value from -100 (full left) to 100 (full right).</param>
        public static void StartSyncTurn(Primitive ports, Primitive speed, Primitive turn)
        {
            int layer, nos;
            DecodePortsDescriptor(ports == null ? "" : ports.ToString(), out layer, out nos);
            int spd = (int)fclamp(speed, -100, 100);
            int trn = (int)fclamp(turn, -100, 100);

            ByteCodeBuffer c = new ByteCodeBuffer();
            c.OP(0xB0);        // turn on synchronized movement
            c.CONST(layer);
            c.CONST(nos);
            c.CONST(spd);
            c.CONST(trn);
            c.CONST(0);
            c.CONST(0);
            EV3RemoteControler.DirectCommand(c, 0, 0);
        }


        /// <summary>
        /// Query the current speed of a single motor.
        /// </summary>
        /// <param name="port">Motor port name</param>
        /// <returns>Current speed in range -100 to 100</returns>
        public static Primitive GetSpeed(Primitive port)
        {
            int layer;
            int no;
            DecodePortDescriptor(port == null ? "" : port.ToString(), out layer, out no);

            if (no < 0)
            {
                return new Primitive(0);
            }
            else
            {
                ByteCodeBuffer c = new ByteCodeBuffer();
                c.OP(0xA8);
                c.CONST(layer);
                c.CONST(no);
                c.GLOBVAR(4);
                c.GLOBVAR(0);
                byte[] reply = EV3RemoteControler.DirectCommand(c, 5, 0);

                int spd = reply == null ? 0 : (sbyte)reply[4];
                return new Primitive(spd);
            }
        }

        /// <summary>
        /// Checks if one or more motors are currently running.
        /// </summary>
        /// <param name="ports">Motor port name(s)</param>
        /// <returns>"True" if at least one of the motors is running, "False" otherwise.</returns>
        public static Primitive IsBusy(Primitive ports)
        {
            int layer;
            int nos;
            DecodePortsDescriptor(ports == null ? "" : ports.ToString(), out layer, out nos);

            ByteCodeBuffer c = new ByteCodeBuffer();
            c.OP(0xA9);
            c.CONST(layer);
            c.CONST(nos);
            c.GLOBVAR(0);
            byte[] reply = EV3RemoteControler.DirectCommand(c, 1, 0);

            return new Primitive((reply != null && reply[0] != 0) ? "True" : "False");
        }

        /// <summary>
        /// Move one or more motors with the specified speed values. The speed can be adjusted along the total rotation to get a soft start and a soft stop if needed.
        /// The total angle to rotate the motor is degrees1+degrees2+degrees3. At the end of the movement, the motor stops automatically (with or without using the brake).
        /// This function returns immediately. You can use IsBusy() to detect the end of the movement or call Wait() to wait until the movement is finished.
        /// </summary>
        /// <param name="ports">Motor port name(s)</param>
        /// <param name="speed">Speed level from -100 (full reverse) to 100 (full forward)</param>
        /// <param name="degrees1">The part of the rotation during which to accelerate</param>
        /// <param name="degrees2">The part of the rotation with uniform motion</param>
        /// <param name="degrees3">The part of the rotation during which to decelerate</param>
        /// <param name="brake">"True", if the motor(s) should switch on the brake after movement</param>
        public static void Schedule(Primitive ports, Primitive speed, Primitive degrees1, Primitive degrees2, Primitive degrees3, Primitive brake)
        {
            int layer, nos;
            DecodePortsDescriptor(ports == null ? "" : ports.ToString(), out layer, out nos);
            int spd = clamp(speed, -100, 100);
            int dgr1 = degrees1;
            int dgr2 = degrees2;
            int dgr3 = degrees3;
            if (dgr1 < 0) dgr1 = -dgr1;
            if (dgr2 < 0) dgr2 = -dgr2;
            if (dgr3 < 0) dgr3 = -dgr3;
            int brk = (brake == null ? "" : brake.ToString()).Equals("true", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

            ByteCodeBuffer c = new ByteCodeBuffer();
            c.OP(0xAE);        // start scheduled movement
            c.CONST(layer);
            c.CONST(nos);
            c.CONST(spd);
            c.CONST(dgr1);
            c.CONST(dgr2);
            c.CONST(dgr3);
            c.CONST(brk);
            EV3RemoteControler.DirectCommand(c, 0, 0);
        }

        /// <summary>
        /// Move one or more motors with the specified power. The power can be adjusted along the total rotation to get a soft start and a soft stop if needed.
        /// The total angle to rotate the motor is degrees1+degrees2+degrees3. At the end of the movement, the motor stops automatically (with or without using the brake).
        /// This function returns immediately. You can use IsBusy() to detect the end of the movement or call Wait() to wait until the movement is finished.
        /// </summary>
        /// <param name="ports">Motor port name(s)</param>
        /// <param name="power">Power level from -100 (full reverse) to 100 (full forward)</param>
        /// <param name="degrees1">The part of the rotation during which to accelerate</param>
        /// <param name="degrees2">The part of the rotation with uniform motion</param>
        /// <param name="degrees3">The part of the rotation during which to decelerate</param>
        /// <param name="brake">"True", if the motor(s) should switch on the brake after movement</param>
        public static void SchedulePower(Primitive ports, Primitive power, Primitive degrees1, Primitive degrees2, Primitive degrees3, Primitive brake)
        {
            int layer, nos;
            DecodePortsDescriptor(ports == null ? "" : ports.ToString(), out layer, out nos);
            int pwr = clamp(power, -100, 100);
            int dgr1 = degrees1;
            int dgr2 = degrees2;
            int dgr3 = degrees3;
            if (dgr1 < 0) dgr1 = -dgr1;
            if (dgr2 < 0) dgr2 = -dgr2;
            if (dgr3 < 0) dgr3 = -dgr3;
            int brk = (brake == null ? "" : brake.ToString()).Equals("true", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

            ByteCodeBuffer c = new ByteCodeBuffer();
            c.OP(0xAC);        // start scheduled movement
            c.CONST(layer);
            c.CONST(nos);
            c.CONST(pwr);
            c.CONST(dgr1);
            c.CONST(dgr2);
            c.CONST(dgr3);
            c.CONST(brk);
            EV3RemoteControler.DirectCommand(c, 0, 0);
        }


        /// <summary>
        /// Move 2 motors synchronously a defined number of degrees. 
        /// The two motors are synchronized which means that when one motor experiences some resistance and cannot keep up its speed, the other motor will also slow down or stop altogether. This is especially useful for vehicles with two independently driven wheels which still need to go straight or make a specified turn.
        /// The distance to move is for the motor with the higher speed.
        /// This function returns immediately. You can use IsBusy() to detect the end of the movement or call Wait() to wait until movement is finished.
        /// </summary>
        /// <param name="ports">Names of 2 motor ports (for example "AB" or "CD"</param>
        /// <param name="speed1">Speed value from -100 (full reverse) to 100 (full forward) of the motor with the lower port letter.</param>
        /// <param name="speed2">Speed value from -100 (full reverse) to 100 (full forward) of the motor with the higher port letter.</param>
        /// <param name="degrees">The angle through which the faster motor should rotate.</param>
        /// <param name="brake">"True", if the motors should switch on the brake after movement.</param>
        public static void ScheduleSync(Primitive ports, Primitive speed1, Primitive speed2, Primitive degrees, Primitive brake)
        {
            double spd1 = fclamp(speed1, -100, 100);
            double spd2 = fclamp(speed2, -100, 100);
            double speed;
            double turn;

            // motor with lower letter is faster or equally fast and must become master      
            if ((spd1 >= 0 ? spd1 : -spd1) >= (spd2 >= 0 ? spd2 : -spd2))
            {
                speed = spd1;
                turn = 100 - ((100.0 * spd2) / spd1);
            }
            // motor with higher letter is faster and must become master
            else
            {
                speed = spd2;
                turn = -(100 - ((100.0 * spd1) / spd2));
            }

            ScheduleSyncTurn(ports, speed, turn, degrees, brake);
        }

        /// <summary>
        /// Move 2 motors synchronously in desired direction a defined number of degrees. 
        /// The two motors are synchronized which means that when one motor experiences some resistance and cannot keep up its speed, the other motor will also slow down or stop altogether. This is especially useful for vehicles with two independently driven wheels which still need to go straight or make a specified turn.
        /// The distance to move is for the motor with the higher speed.
        /// This function returns immediately. You can use IsBusy() to detect the end of the movement or call Wait() to wait until movement is finished.
        /// </summary>
        /// <param name="ports">Names of 2 motor ports (for example "AB" or "CD"</param>
        /// <param name="speed">Speed value from -100 (full reverse) to 100 (full forward) of the motors.</param>
        /// <param name="turn">Turn value from -100 (full left) to 100 (full right).</param>
        /// <param name="degrees">The angle through which the faster motor should rotate.</param>
        /// <param name="brake">"True", if the motors should switch on the brake after movement.</param>
        public static void ScheduleSyncTurn(Primitive ports, Primitive speed, Primitive turn, Primitive degrees, Primitive brake)
        {
            int layer, nos;
            DecodePortsDescriptor(ports == null ? "" : ports.ToString(), out layer, out nos);
            int spd = (int)fclamp(speed, -100, 100);
            int trn = (int)fclamp(turn, -100, 100);
            int dgr = degrees;
            if (dgr < 0) dgr = -dgr;
            int brk = (brake == null ? "" : brake.ToString()).Equals("true", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

            if (dgr > 0)
            {
                ByteCodeBuffer c = new ByteCodeBuffer();
                c.OP(0xB0);        // start scheduled command
                c.CONST(layer);
                c.CONST(nos);
                c.CONST(spd);
                c.CONST(trn);
                c.CONST(dgr);
                c.CONST(brk);
                EV3RemoteControler.DirectCommand(c, 0, 0);
            }
        }


        /// <summary>
        /// Set the rotation count of one or more motors to 0.
        /// </summary>
        /// <param name="ports">Motor port name(s)</param>
        public static void ResetCount(Primitive ports)
        {
            int layer, nos;
            DecodePortsDescriptor(ports == null ? "" : ports.ToString(), out layer, out nos);

            ByteCodeBuffer c = new ByteCodeBuffer();
            c.OP(0xB2);
            c.CONST(layer);
            c.CONST(nos);
            EV3RemoteControler.DirectCommand(c, 0, 0);
        }

        /// <summary>
        /// Query the current rotation count of a single motor. 
        /// As long as the counter is not reset it will accurately measure all movements of a motor, even if the motor is driven by some external force while not actively running.
        /// </summary>
        /// <param name="port">Motor port name</param>
        /// <returns>The current rotation count in degrees.</returns>
        public static Primitive GetCount(Primitive port)
        {
            int layer, no;
            DecodePortDescriptor(port == null ? "" : port.ToString(), out layer, out no);

            if (no < 0)
            {
                return new Primitive(0);
            }
            else
            {
                ByteCodeBuffer c = new ByteCodeBuffer();
                c.OP(0xB3);
                c.CONST(layer);
                c.CONST(no);
                c.GLOBVAR(0);
                byte[] reply = EV3RemoteControler.DirectCommand(c, 4, 0);

                int tacho = 0;
                if (reply != null)
                {
                    tacho = ((int)reply[0]) | (((int)reply[1]) << 8) | (((int)reply[2]) << 16) | (((int)reply[3]) << 24);
                }
                return new Primitive(tacho);
            }
        }

        /// <summary>
        /// Move one or more motors with the specified speed the specified angle (in degrees).
        /// This command will block execution until the motor has reached its destination.
        /// When you need finer control over the movement (soft acceleration or deceleration), consider using the command Motor.Schedule instead.
        /// </summary>
        /// <param name="ports">Motor port name(s)</param>
        /// <param name="speed">Speed level from -100 (full reverse) to 100 (full forward)</param>
        /// <param name="degrees">The angle to rotate</param>
        /// <param name="brake">"True", if the motor(s) should switch on the brake after movement</param>
        public static void Move(Primitive ports, Primitive speed, Primitive degrees, Primitive brake)
        {
            Schedule(ports, speed, new Primitive(0), degrees, new Primitive(0), brake);
            Wait(ports);
        }

        /// <summary>
        /// Move one or more motors with the specified power the specified angle (in degrees).
        /// This command will block execution until the motor has reached its destination.
        /// When you need finer control over the movement (soft acceleration or deceleration), consider using the command Motor.SchedulePower instead.
        /// </summary>
        /// <param name="ports">Motor port name(s)</param>
        /// <param name="power">Power level from -100 (full reverse) to 100 (full forward)</param>
        /// <param name="degrees">The angle to rotate</param>
        /// <param name="brake">"True", if the motor(s) should switch on the brake after movement</param>
        public static void MovePower(Primitive ports, Primitive power, Primitive degrees, Primitive brake)
        {
            SchedulePower(ports, power, new Primitive(0), degrees, new Primitive(0), brake);
            Wait(ports);
        }

        /// <summary>
        /// Moves 2 motors synchronously a defined number of degrees. 
        /// The two motors are synchronized which means that when one motor experiences some resistance and cannot keep up its speed, the other motor will also slow down or stop altogether. This is especially useful for vehicles with two independently driven wheels which still need to go straight or make a specified turn.
        /// The angle to move is for the motor with the higher speed.
        /// </summary>
        /// <param name="ports">Names of 2 motor ports (for example "AB" or "CD"</param>
        /// <param name="speed1">Speed value from -100 (full reverse) to 100 (full forward) of the motor with the lower port letter.</param>
        /// <param name="speed2">Speed value from -100 (full reverse) to 100 (full forward) of the motor with the higher port letter.</param>
        /// <param name="degrees">The angle of the faster motor to rotate</param>
        /// <param name="brake">"True", if the motors should switch on the brake after movement</param>
        public static void MoveSync(Primitive ports, Primitive speed1, Primitive speed2, Primitive degrees, Primitive brake)
        {
            ScheduleSync(ports, speed1, speed2, degrees, brake);
            Wait(ports);
        }

        /// <summary>
        /// Moves 2 motors synchronously into desired direction a defined number of degrees. 
        /// The two motors are synchronized which means that when one motor experiences some resistance and cannot keep up its speed, the other motor will also slow down or stop altogether. This is especially useful for vehicles with two independently driven wheels which still need to go straight or make a specified turn.
        /// The angle to move is for the motor with the higher speed.
        /// </summary>
        /// <param name="ports">Names of 2 motor ports (for example "AB" or "CD"</param>
        /// <param name="speed">Speed value from -100 (full reverse) to 100 (full forward) of the motors.</param>
        /// <param name="turn">Turn value from -100 (full left) to 100 (full right).</param>
        /// <param name="degrees">The angle of the faster motor to rotate</param>
        /// <param name="brake">"True", if the motors should switch on the brake after movement</param>
        public static void MoveSyncTurn(Primitive ports, Primitive speed, Primitive turn, Primitive degrees, Primitive brake)
        {
            ScheduleSyncTurn(ports, speed, turn, degrees, brake);
            Wait(ports);
        }


        /// <summary>
        /// Wait until the specified motor(s) has finished a "Schedule..." or "Move..." operation.
        /// Using this method is normally better than calling IsBusy() in a tight loop.
        /// </summary>
        /// <param name="ports">Motor port name(s)</param>
        public static void Wait(Primitive ports)
        {
            int layer, nos;
            DecodePortsDescriptor(ports == null ? "" : ports.ToString(), out layer, out nos);

            ByteCodeBuffer c = new ByteCodeBuffer();
            c.Clear();
            c.OP(0xA9);
            c.CONST(layer);
            c.CONST(nos);
            c.GLOBVAR(0);

            for (;;)
            {
                byte[] reply = EV3RemoteControler.DirectCommand(c, 1, 0);
                if (reply == null || reply[0] == 0)
                {
                    break;
                }
                System.Threading.Thread.Sleep(2);
            }
        }



        private static void DecodePortDescriptor(String descriptor, out int layer, out int no)
        {
            layer = 0;
            no = -1;
            for (int i = 0; i < descriptor.Length; i++)
            {
                switch (descriptor[i])
                {
                    case '2': layer = 1; break;
                    case '3': layer = 2; break;
                    case '4': layer = 3; break;
                    case 'a':
                    case 'A': no = 0; break;
                    case 'b':
                    case 'B': no = 1; break;
                    case 'c':
                    case 'C': no = 2; break;
                    case 'd':
                    case 'D': no = 3; break;
                }
            }
        }

        private static void DecodePortsDescriptor(String descriptor, out int layer, out int nos)
        {
            layer = 0;
            nos = 0;
            for (int i = 0; i < descriptor.Length; i++)
            {
                switch (descriptor[i])
                {
                    case '2': layer = 1; break;
                    case '3': layer = 2; break;
                    case '4': layer = 3; break;
                    case 'a':
                    case 'A': nos = nos | 1; break;
                    case 'b':
                    case 'B': nos = nos | 2; break;
                    case 'c':
                    case 'C': nos = nos | 4; break;
                    case 'd':
                    case 'D': nos = nos | 8; break;
                }
            }
        }

        private static int clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }
        private static double fclamp(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }
    }

}
