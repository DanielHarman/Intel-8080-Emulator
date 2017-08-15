using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _8080Emu
{


    public class _8080CPU
    {

        public byte[] memory;
        public byte[] devices;
        public byte reg_a, reg_b, reg_c;
        public byte reg_d, reg_e, reg_h;
        public byte reg_l, reg_flags;
        public ushort stackPointer, programCounter;
        public bool running;

        private bool jumped = false;
        // For setting up proper cycles later
        private bool loadingImmediate = false;
        private bool loadingImmediateAddress = false;


        public _8080CPU()
        {
            //8080 can address 64k of ram
            memory = new byte[65536];
            devices = new byte[255];
            reg_a = reg_b = reg_c = reg_d = 0x0;
            reg_e = reg_h = reg_l = 0x0;

            reg_flags = 0b00001000;
            
            running = true;
            stackPointer = 0xFF; //Just a random location
            programCounter = 0;
        }

        public  void loadMemory(string filename)
        {
            BinaryReader binReader = new BinaryReader(File.OpenRead(filename));

            for (int i = 0x0; i <= 0xffff; i++)
            {
                memory[i] = binReader.ReadByte();
            }
            binReader.Close();
        }

        public void execute()
        {
            //Execute instructions at PC
            //Read byte from memory
            byte instruction = memory[programCounter];
            jumped = false;

            Console.WriteLine("Instruction: 0x" + programCounter.ToString("X4") + " 0x" + instruction.ToString("X2"));
            Console.WriteLine("SP: 0x{0}", stackPointer.ToString("X4"));

            if (instruction == 0)
            {
                //NOP
                Console.WriteLine("NOP");
            }
            else if (instruction == 0b01110110)
            {
                //HLT
                Console.WriteLine("HLT");
                running = false;

            }
            else if (instruction  == 0b11001110)
            {
                //ACI
                Console.WriteLine("ACI");
                aci();
                programCounter++;
            }
            else if (instruction == 0b11000110)
            {
                //ADI
                Console.WriteLine("ADI");
                adi();
                programCounter++;
            }
            else if ((instruction & 0b11111000) == 0b10010000)
            {
                //ADC
                Console.WriteLine("ADC");
                adc(instruction);
            }

            else if ((instruction & 0b11111000) == 0b10100000)
            {
                //ANA
                Console.WriteLine("ANA");
                ana(instruction);
            }

            else if ((instruction & 0b11000000) == 0b01000000)
            {
                //MOV
                Console.WriteLine("MOV");
                movRegReg(instruction);
            }
            else if ((instruction & 0b11000111) == 0b00000110)
            {
                //MVI
                Console.WriteLine("MVI");
                mvi(instruction);
                programCounter++; //Skip next byte?
            }
            else if ((instruction & 0b11111000) == 0b10000000)
            {
                //ADD
                Console.WriteLine("ADD");
                add(instruction);
            }
            else if ((instruction & 0b11111000) == 0b10011000)
            {
                //SBB
                Console.WriteLine("SBB");
                sbb(instruction);
            }
            else if ((instruction & 0b11111000) == 0b10010000)
            {
                //SUB
                Console.WriteLine("SUB");
                sub(instruction);
            }
            else if ((instruction & 0b11111000) == 0b10100000)
            {
                //AND
                Console.WriteLine("AND");
                and(instruction);
            }
            else if ((instruction & 0b11111000) == 0b10111000)
            {
                //CMP
                Console.WriteLine("CMP");
                compare(instruction);
            }
            else if (instruction == 0b11000011)
            {
                //JMP
                Console.WriteLine("JMP");
                jump();
            }
            else if ((instruction & 0b11000111) == 0b11000010)
            {
                //Jump instructions
                Console.WriteLine("JMP");
                switch (instruction & 0b00111000)
                {
                    case 0:
                        //jnz
                        jumpIfNotZero();
                        break;
                    case 0b00001000:
                        //JZ
                        jumpIfZero();
                        break;
                    case 0b00010000:
                        //JNC
                        jumpIfNoCarry();
                        break;
                    case 0b00011000:
                        //JC
                        jumpIfCarry();
                        break;

                    case 0b00100000:
                        //JPO
                        jumpIfParityOdd();
                        break;
                    case 0b00101000:
                        //JPE
                        jumpIfParityEven();
                        break;
                    case 0b00110000:
                        //JP
                        jumpIfPositive();
                        break;
                    case 0b00111000:
                        //JP
                        jumpIfMinus();
                        break;

                }
                if (!jumped)
                    programCounter += 2; //Jump over the address
            }
            else if ((instruction & 0b11001111) == 0b11000101)
            {
                //PUSH
                Console.WriteLine("PUSH");
                push(instruction);
            }
            else if ((instruction & 0b11001111) == 0b11000001)
            {
                //POP
                Console.WriteLine("POP");
                pop(instruction);
            }
            else if ((instruction & 0b11101111) == 0b00000010)
            {
                //STAX
                Console.WriteLine("STAX");
            }
            else if ((instruction & 0b11101111) == 0b00001010)
            {
                //LDAX
                Console.WriteLine("LDAX");
                LoadAccumlator(instruction);
            }
            else if ((instruction & 0b11001111) == 0b00000001)
            {
                //LXI
                Console.WriteLine("LXI");
                lxi(instruction);
                programCounter += 2;
            }
            else if ((instruction == 0b00110111))
            {
                //STC
                Console.WriteLine("STC");
                setCF(true);
            }
            else if (instruction== 0b11011011)
            {
                //IN
                Console.WriteLine("IN");
                cpu_in();
                programCounter++; //Skip next byte?
            }
            else if ((instruction & 0b11000111) == 0b00000100)
            {
                //INR
                Console.WriteLine("INR");
                increment(instruction);
            }
            else if ((instruction & 0b11001111) == 0b00001001)
            {
                //DAD
                Console.WriteLine("DAD");
                dad(instruction);
            }
            else if ((instruction & 0b11000111) == 0b00000101)
            {
                //DCR
                Console.WriteLine("DCR");
                decrement(instruction);
            }
            else if (instruction == 0b11001101)
            {
                //CALL
                Console.WriteLine("CALL");
            }
            else if ((instruction & 0b11000111) == 0b00000101)
            {
                //Sub calls
                Console.WriteLine("CALL");
            }
            else if ((instruction & 0b11111000) == 0b10110000)
            {
                //ORA
                Console.WriteLine("ORA");
                ora(instruction);
            }
            else if (instruction == 0b11010011)
            {
                //OUT
                Console.WriteLine("OUT");
                cpu_out();
                programCounter++; //Skip next byte?
            }
            else if (instruction == 0x0F)
            {
                Console.WriteLine("RRC");
                rrc();
            }
            else if ((instruction & 0b11111000) == 0b10101000)
            {
                //XRA
                Console.WriteLine("XRA");
                xra(instruction);
            }
            else
            {
                Console.WriteLine("NON IMPLEMENTED OR INCORRECT INSTRUCTION ->{0}<-", instruction.ToString("X2"));
            }
            if (!jumped)
                programCounter++;
        }

       private byte readMemory(ushort address)
        {
            return 0;
        }

        private byte readDevice(byte device)
        {
            return 0;
        }

        private void writeDevice(byte device)
        {
            
        }

        private bool checkIsMinus(byte number)
        {
            //MSB indicates sign
            return (number & 0b10000000) == 0b10000000;
        }

        private bool hasParity(byte number)
        {
            byte y;
            y = (byte)(number ^ (number >> 1));
            y = (byte)(y ^ (y >> 2));
            y = (byte)(y ^ (y >> 4));
            y = (byte)(y ^ (y >> 8));
            y = (byte)(y ^ (y >> 16));
            return (y & 1) == 0;
        }

        private void movRegReg(byte instruction)
        {

            byte destRegNum = (byte)(instruction & 0b00111000);
            byte srcRegNum = (byte)(instruction & 0b00000111);

            ///This is where variable references would be useful
            switch (destRegNum)
            {
                case 0b111000:
                    //A
                    reg_a = getRegValue(srcRegNum);
                    break;

                case 0b000000:
                    //B
                    reg_b = getRegValue(srcRegNum);
                    break;

                case 0b001000:
                    //C
                    reg_c = getRegValue(srcRegNum);
                    break;

                case 0b010000:
                    //D
                    reg_d = getRegValue(srcRegNum);
                    break;

                case 0b011000:
                    //E
                    reg_e = getRegValue(srcRegNum);
                    break;

                case 0b100000:
                    //H
                    reg_h = getRegValue(srcRegNum);
                    break;

                case 0b101000:
                    //L
                    reg_l = getRegValue(srcRegNum);
                    break;

                case 0b110000:
                    //Memory reference from H:L
                    memory[reg_h << 8 | reg_l] = getRegValue(srcRegNum);
                    break;
            }
        }

        private byte getRegValue(byte srcRegNum)
        {
            //I don't like this much
            switch (srcRegNum)
            {
                case 0b111:
                    //A
                    return reg_a;

                case 0b000:
                    //B
                    return reg_b;

                case 0b001:
                    //C
                    return reg_c;

                case 0b010:
                    //D
                    return reg_d;

                case 0b011:
                    //E
                    return reg_e;

                case 0b100:
                    //H
                    return reg_h;

                case 0b101:
                    //L
                    return reg_l;

                case 0b110:
                    //Memory reference from H:L
                    return memory[reg_h << 8 | reg_l];
            }
            return 0xCC;
        }

        private ushort getRegPairVal(byte regPairNum)
        {
            switch (regPairNum)
            {
                case 0:
                    //BC
                    return memory[(reg_b << 8 | reg_c)];
                case 0b01:
                    //DE
                    return memory[(reg_d << 8 | reg_e)];
                case 0b10:
                    //HL
                    return memory[(reg_h << 8 | reg_l)];
                case 0b11:
                    //SP
                    return stackPointer;

            }
            return 0xCC;
        }

        private void mvi(byte instruction)
        {
            switch (instruction & 0b00111000)
            {
                case 0b111000:
                    //A
                    reg_a = memory[programCounter + 1];
                    break;

                case 0b000000:
                    //B
                    reg_b = memory[programCounter + 1];
                    break;

                case 0b001000:
                    //C
                    reg_c = memory[programCounter + 1];
                    break;

                case 0b010000:
                    //D
                    reg_d = memory[programCounter + 1];
                    break;

                case 0b011000:
                    //E
                    reg_e = memory[programCounter + 1];
                    break;

                case 0b100000:
                    //H
                    reg_h = memory[programCounter + 1];
                    break;

                case 0b101000:
                    //L
                    reg_l = memory[programCounter + 1];
                    break;

                case 0b110000:
                    //Memory reference from H:L
                    memory[reg_h << 8 | reg_l] = memory[programCounter + 1];
                    break;
            }

        }

        public void add(byte instruction)
        {

            byte result = getRegValue((byte)(instruction & 0b00000111));

            setCF((result + reg_a) > 0xFF);
            setSF(checkIsMinus((byte)(result + reg_a)));
            setZF((byte)(reg_a + result) == 0b0);
            setPF(hasParity((byte)(reg_a + result)));

            reg_a += result;
            setZF(reg_a == 0);
        }

        public void aci()
        {
            byte result = (byte)(memory[programCounter + 1] + ((getCF() ? 0b1 : 0b0)));

            setCF((result + reg_a) > 0xFF);
            setSF(checkIsMinus((byte)(result+reg_a)));
            setZF((byte)(reg_a + result) == 0b0);
            setPF(hasParity((byte)(reg_a + result)));

            reg_a += result ;

        }
        public void adc(byte instruction)
        {
            byte result = (byte)(getRegValue((byte)(instruction & 0b00000111)) + ((getCF() ? 0b1 : 0b0)));

            setCF((result + reg_a) > 0xFF);
            setSF(checkIsMinus((byte)(result + reg_a)));
            setZF((byte)(reg_a + result) == 0b0);
            setPF(hasParity((byte)(reg_a + result)));

            reg_a += result;
            setZF(reg_a == 0);
        }

        public void adi()
        {
            byte result = memory[programCounter + 1];

            setCF((result + reg_a) > 0xFF);
            setSF(checkIsMinus((byte)(result + reg_a)));
            setZF((byte)(reg_a + result) == 0b0);
            setPF(hasParity((byte)(reg_a + result)));

            reg_a += result;
        }

        public void ana(byte instruction)
        {
            byte result = getRegValue((byte)(instruction & 0b00000111));

            setCF(false);
            setSF(checkIsMinus((byte)(result & reg_a)));
            setZF((byte)(reg_a & result) == 0b0);
            setPF(hasParity((byte)(reg_a & result)));

            reg_a &= result;
        }

        public void ani()
        {

        }

        public void cma(byte instruction)
        {

        }

        public void cmc(byte instruction)
        {

        }

        public void daa(byte instruction)
        {

        }

        public void dad(byte instruction)
        {
            ushort pairValue = getRegPairVal((byte)((instruction & 0b00110000) >> 4) );

            ushort hlValue = (ushort)((reg_h << 8) + reg_l);
            setCF((hlValue + pairValue) > 0xFFFF);
            ushort result = (ushort)(hlValue + pairValue);
            reg_h = (byte)(result >> 8);
            reg_l = (byte)(result);
        }

        public void dcx(byte instruction)
        {

        }

        public void di(byte instruction)
        {

        }

        public void ei(byte instruction)
        {

        }

        public void cpu_in()
        {
            reg_a = devices[memory[programCounter + 1]];
        }

        public void inx(byte instruction)
        {

        }

        public void lda(byte instruction)
        {

        }

        public void lhld(byte instruction)
        {

        }

        public void lxi(byte instruction)
        {
            switch (instruction & 0b00110000)
            {
                case 0b0:
                    //B:C
                    reg_b = memory[programCounter + 1];
                    reg_c = memory[programCounter + 2];
                    break;
                case 0b00010000:
                    //D:E
                    reg_d = memory[programCounter + 1];
                    reg_e = memory[programCounter + 2];
                    break;
                case 0b00100000:
                    //HL
                    reg_h = memory[programCounter + 1];
                    reg_l = memory[programCounter + 2];
                    break;
                case 0b00110000:
                    //SP
                    stackPointer = (ushort)((memory[programCounter + 2] << 8) + memory[programCounter + 1]);
                    break;
            }
        }

        public void ora(byte instruction)
        {
            byte result = getRegValue((byte)(instruction & 0b00000111));

            setCF(false);
            setSF(checkIsMinus((byte)(result | reg_a)));
            setZF((byte)(reg_a | result) == 0b0);
            setPF(hasParity((byte)(reg_a | result)));

            reg_a |= result;
        }

        public void ori(byte instruction)
        {

        }

        public void cpu_out()
        {
            devices[memory[programCounter + 1]] = reg_a;
        }

        public void pchl(byte instruction)
        {

        }

        public void ral(byte instruction)
        {

        }

        public void rar(byte instruction)
        {

        }

        public void ret(byte instruction)
        {

        }

        public void rnz(byte instruction)
        {

        }

        public void rz(byte instruction)
        {

        }

        public void rnc(byte instruction)
        {

        }

        public void rc(byte instruction)
        {

        }

        public void rpo(byte instruction)
        {

        }

        public void rpe(byte instruction)
        {

        }

        public void rp(byte instruction)
        {

        }

        public void rm(byte instruction)
        {

        }

        public void rlc(byte instruction)
        {

        }

        public void rrc()
        {
            byte carry = (byte)(reg_a & 0b00000001);
            setCF(carry == 1);
            reg_a = (byte)((reg_a >> 1) + (carry << 8));
        }

        public void rst(byte instruction)
        {

        }

        public void sbb(byte instruction)
        {
            byte result = (byte)(getRegValue((byte)(instruction & 0b00000111)) + ((getCF() ? 0b1 : 0b0)));

            result = (byte) ((result ^ 0xFF)+1); //two's compliment

            setSF(checkIsMinus((byte)(result + reg_a)));
            setZF((byte)(reg_a + result) == 0b0);
            setPF(hasParity((byte)(reg_a + result)));

            reg_a += result;
            setZF(reg_a == 0);
        }

        public void sbi(byte instruction)
        {

        }

        public void shld(byte instruction)
        {

        }

        public void sphl(byte instruction)
        {

        }

        public void sta(byte instruction)
        {

        }

        public void stc(byte instruction)
        {

        }

        public void sui(byte instruction)
        {

        }

        public void xchg(byte instruction)
        {

        }

        public void xra(byte instruction)
        {
            byte result = getRegValue((byte)(instruction & 0b00000111));

            setCF(false);
            setSF(checkIsMinus((byte)(result ^ reg_a)));
            setZF((byte)(reg_a ^ result) == 0b0);
            setPF(hasParity((byte)(reg_a ^ result)));

            reg_a ^= result;

        }
        public void xri(byte instruction)
        {

        }

        public void xthl(byte instruction)
        {

        }

        public void sub(byte instruction)
        {
            byte result = getRegValue((byte)(instruction & 0b00000111));

            setSF(checkIsMinus((byte)(result - reg_a)));
            setZF((byte)(reg_a - result) == 0b0);
            setPF(hasParity((byte)(reg_a - result)));

            reg_a -= result;
            setZF(reg_a == 0);

        }

        public void and(byte instruction)
        {
            reg_a &= getRegValue((byte)(instruction & 0b00000111));
        }

        public void or(byte instruction)
        {
            reg_a |= getRegValue((byte)(instruction & 0b00000111));
        }

        public void xor(byte instruction)
        {
            reg_a ^= getRegValue((byte)(instruction & 0b00000111));
        }

        public void rotateRight(byte instruction)
        {
            reg_a ^= getRegValue((byte)(instruction & 0b00000111));
        }

        public void rotateLeft(byte instruction)
        {
            reg_a ^= getRegValue((byte)(instruction & 0b00000111));
        }

        public void jump()
        {
            //Jump to the address contained in the next two bytes of memory
            Console.WriteLine("Jumping to 0x{0}", ((ushort)(memory[programCounter + 2] << 8 | memory[programCounter + 1])).ToString("X4"));
            programCounter = (ushort)(memory[programCounter + 2] << 8 | memory[programCounter + 1]);// memory[reg_h << 8 | reg_l]
            jumped = true;
        }

        public void LoadProgCounter()
        {
            //Jump to address held in H:L
            programCounter = (byte)(reg_h << 8 | reg_l);
        }

        public void jumpIfCarry()
        {
            if (getCF())
            {
                jump();
            }
        }

        public void jumpIfNoCarry()
        {
            if (!getCF())
            {
                jump();
            }
        }

        public void jumpIfZero()
        {
            if (getZF())
            {
                jump();
            }
        }

        public void jumpIfNotZero()
        {
            if (!getZF())
            {
                jump();
            }
        }

        public void jumpIfMinus()
        {
            if (getSF())
            {
                jump();
            }
        }

        public void jumpIfPositive()
        {
            if (!getSF())
            {
                jump();
            }
        }

        public void jumpIfParityEven()
        {
            if (getPF())
            {
                jump();
            }
        }

        public void jumpIfParityOdd()
        {
            if (!getPF())
            {
                jump();
            }
        }

        public void call()
        {

        }

        public void LoadAccumlator(byte instruction)
        {
            if ((instruction & 0b00010000) == 0)
            {
                //BC
                reg_a = memory[(reg_b << 8 | reg_c)];
            }
            else
            {
                //DE
                reg_a = memory[(reg_d << 8 | reg_e)];
            }
        }

        public void StoreAccumlator(byte instruction)
        {
            if ((instruction & 0b00010000) == 0)
            {
                //BC
                memory[(reg_b << 8 | reg_c)] = reg_a;
            }
            else
            {
                //DE
                memory[(reg_d << 8 | reg_e)] = reg_a;
            }
        }

        public void push(byte instruction)
        {

            //Find RP
            switch (instruction & 0b00110000)
            {
                case 0:
                    //BC C before B
                    memory[--stackPointer] = reg_b;
                    memory[--stackPointer] = reg_c;
                    break;
                case 0b00010000:
                    //DE
                    memory[--stackPointer] = reg_d;
                    memory[--stackPointer] = reg_e;
                    break;
                case 0b00100000:
                    //HL
                    memory[--stackPointer] = reg_h;
                    memory[--stackPointer] = reg_l;
                    break;
                case 0b00110000:
                    //FLAGS + A
                    memory[--stackPointer] = reg_flags;
                    memory[--stackPointer] = reg_a;
                    break;
            }
        }

        public void pop(byte instruction)
        {

            //Find RP
            switch (instruction & 0b00110000)
            {
                case 0:
                    //BC C before B
                    reg_c = memory[stackPointer++];
                    reg_b = memory[stackPointer++];
                    break;
                case 0b00010000:
                    //DE
                    reg_e = memory[stackPointer++];
                    reg_d = memory[stackPointer++];
                    break;
                case 0b00100000:
                    //HL
                    reg_l = memory[stackPointer++];
                    reg_h = memory[stackPointer++];
                    break;
                case 0b00110000:
                    //FLAGS + A
                    reg_a = memory[stackPointer++];
                    reg_flags = memory[stackPointer++];
                    break;
            }

        }

        public void compare(byte instruction)
        {
            int val = reg_a - getRegValue((byte)(instruction & 0b00000111));

            setSF(val < 0);
            setZF(val == 0);

        }

        public void increment(byte instruction)
        {
            switch (instruction & 0b00111000)
            {
                case 0b111000:
                    //A
                    reg_a++;
                    break;

                case 0b000000:
                    //B
                    reg_b++;
                    break;

                case 0b001000:
                    //C
                    reg_c++;
                    break;

                case 0b010000:
                    //D
                    reg_d++;
                    break;

                case 0b011000:
                    //E
                    reg_e++;
                    break;

                case 0b100000:
                    //H
                    reg_h++;
                    break;

                case 0b101000:
                    //L
                    reg_l++;
                    break;

                case 0b110000:
                    //Memory reference from H:L
                    memory[reg_h << 8 | reg_l]++;
                    break;
            }

        }

        public void decrement(byte instruction)
        {
            switch (instruction & 0b00111000)
            {
                case 0b111000:
                    //A
                    reg_a--;
                    break;

                case 0b000000:
                    //B
                    reg_b--;
                    break;

                case 0b001000:
                    //C
                    reg_c--;
                    break;

                case 0b010000:
                    //D
                    reg_d--;
                    break;

                case 0b011000:
                    //E
                    reg_e--;
                    break;

                case 0b100000:
                    //H
                    reg_h--;
                    break;

                case 0b101000:
                    //L
                    reg_l--;
                    break;

                case 0b110000:
                    //Memory reference from H:L
                    memory[reg_h << 8 | reg_l]--;
                    break;
            }

        }

        public void setSF(bool flag)
        {
            if (flag)
                reg_flags = (byte)(reg_flags | 0b10000000);
            else
                reg_flags = (byte)(reg_flags & 0b01111111);
        }

        public void setZF(bool flag)
        {
            if (flag)
                reg_flags = (byte)(reg_flags | 0b01000000);
            else
                reg_flags = (byte)(reg_flags & 0b10111111);
        }

        public void setCF(bool flag)
        {
            if (flag)
                reg_flags = (byte)(reg_flags | 0b00000001);
            else
                reg_flags = (byte)(reg_flags & 0b11111110);
        }

        public void setPF(bool flag)
        {
            if (flag)
                reg_flags = (byte)(reg_flags | 0b00000100);
            else
                reg_flags = (byte)(reg_flags & 0b11111011);
        }

        public bool getZF()
        {
            return (reg_flags & 64) == 64;
        }

        public bool getSF()
        {
            return (reg_flags & 128) == 128; // Binary and with 0b10000000
        }

        public bool getACF()
        {
            return (reg_flags & 16) == 16;
        }

        public bool getCF()
        {
            return (reg_flags & 1) == 1;
        }

        public bool getPF()
        {
            return (reg_flags & 4) == 4;
        }

    }
}
