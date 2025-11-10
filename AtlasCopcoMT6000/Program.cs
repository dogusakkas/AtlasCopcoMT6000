using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AtlasCopcoMT6000
{
    internal class Program
    {
        private TcpClient client;
        private NetworkStream stream;


        static void Main(string[] args)
        {
            //string ipAddress = "10.145.185.110"; // Cihazın IP adresi
            string ipAddress = "10.145.204.57"; // Cihazın IP adresi
            int port = 4545; // Cihazın port numarası

            try
            {
                TcpClient client = new TcpClient(ipAddress, port);
                NetworkStream stream = client.GetStream();
                int parameterSetID = 1;

                Console.WriteLine("Bağlantı kuruldu, veri dinleniyor...");

                // MID01 komutunu göndererek bağlantıyı başlat
                MID01(stream, client);

                //// PSET SEÇİMİ YAPILMAK İSTENMİYOR İSE MID10 VE MID18 KAPATILABİLİR
                ///

                // 8 Dijital Input'u dinleme (0210)
                //MID0210(stream, client);

                // Tek bir dijital input fonksiyonunu dinleme (0220)  // BURADA KENDİ INPUT KODUMUZU YOLLAMAMIZ GEREKEBİLİR - RELEASE TOOL İÇİN
                //MID0220(stream, client);

                // Röle fonksiyonlarını dinleme (0217)
                MID0217(stream, client);

                // MID0010 Get Pset List
                //MID0010(stream, client);

                // MID0018 ile pset ayarı yap
                //MID0018(stream, client,  parameterSetID);

                // MID08 ile veri almaya başla
                //MID08(stream, client);

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
            }
        }


        #region MID0210 - Subscribe to externally monitored inputs
        static void MID0210(NetworkStream stream, TcpClient client)
        {
            Console.WriteLine("\nMID0210 (Subscribe external inputs) Gönderiliyor...");
            string msg = "002900080010        021100100\0"; // 0020 = 20 byte uzunluk, MID=0211 // MID 211 KODU
            //string msg = "00200210            \0"; // 0020 = 20 byte uzunluk, MID=0210
            SendAndReceive(stream, client, msg);
        }
        #endregion

        #region MID0211 - Status externally monitored inputs (Response)
        static void MID0211Handler(string data)
        {
            // Gelen 8 inputun durumunu çözümleme
            Console.WriteLine("\n-> MID0211 Dijital Input Durumları:");
            for (int i = 21; i <= 28; i++)
            {
                int inputNo = i - 20;
                char status = data.Length > i ? data[i] : '?';
                Console.WriteLine($"DIG/IN {inputNo}: {(status == '1' ? "ON" : "OFF")}");
            }
        }
        #endregion

        #region MID0217 - Relay function upload
        static void MID0217(NetworkStream stream, TcpClient client)
        {
            Console.WriteLine("\nMID0217 (Relay function) Gönderiliyor...");

            // 3 farklı relay fonksiyonu için MID mesajları
            var messages = new[]
            {
        "003200080010        021700103030\0", // EXTERNAL MONITORED 1
        "003200080010        021700103031\0", // EXTERNAL MONITORED 2
        "003200080010        021700103032\0",  // EXTERNAL MONITORED 3
        "003200080010        021700103033\0"  // EXTERNAL MONITORED 4
    };

            foreach (var msg in messages)
            {
                Console.WriteLine($"\n-> Gönderiliyor: {msg}");
                SendAndReceive(stream, client, msg);
                Task.Delay(200).Wait(); 
            }
        }

        static void MID0217Handler(string data)
        {
            try
            {
                string status = "";
                // Gelen data'nın uzunluğunu kontrol et
                if (data.Length < 28)
                {
                    Console.WriteLine("⚠ MID0217 veri uzunluğu yetersiz, parse edilemiyor.");
                    return;
                }

                if (data.Length > 30)
                {
                    status = data.Substring(52, 1);

                }
                else
                {
                    status = data.Substring(27, 1); // 2 DATA GELDİĞİ İÇİN GEÇİCİ OLARAK BUNU KAPATTIM 

                }

                string relayNumber = data.Substring(22, 3);

                // data = "002400050010    00  0008\0002802170010    00  01030020\0"

                Console.WriteLine($"-> Röle {relayNumber} durumu: {(status == "1" ? "AKTİF" : "PASİF")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("MID0217 parse hatası: " + ex.Message);
            }
        }
        #endregion


        #region MID0217 - Relay function upload
        //static void MID0217(NetworkStream stream, TcpClient client)
        //{
        //    Console.WriteLine("\nMID0217 (Relay function) Gönderiliyor...");
        //    //string msg = "003200080010        021700103001\0"; // 0032 = 32 byte uzunluk, MID=0217
        //    string msg = "003200080010        021700103030\0"; // 0032 = 32 byte uzunluk, MID=0217 // EXTERNAL MONITORED 1
        //    string msg2 = "003200080010        021700103031\0"; // 0032 = 32 byte uzunluk, MID=0217 // EXTERNAL MONITORED 2
        //    string msg3 = "003200080010        021700103032\0"; // 0032 = 32 byte uzunluk, MID=0217 // EXTERNAL MONITORED 3
        //    SendAndReceive(stream, client, msg);
        //}

        //static void MID0217Handler(string data)
        //{
        //    string relayNumber = data.Substring(22, 3);
        //    string status = data.Substring(27, 1);
        //    Console.WriteLine($"-> Röle {relayNumber} durumu: {(status == "1" ? "AKTİF" : "PASİF")}");
        //}
        #endregion

        #region MID0220 - Subscribe to single digital input function
        static void MID0220(NetworkStream stream, TcpClient client)
        {
            Console.WriteLine($"\nMID0220 (Subscribe digital input 050) Gönderiliyor...");

            //string inputNo = digitalInputNo.ToString("D3"); // Üç haneli ASCII
            string msg = $"003200080010        022100103050\0"; // EXTERNAL MONITORED 1
            //string msg = $"00230220            {inputNo}\0";
            SendAndReceive(stream, client, msg);
        }
        #endregion

        #region MID0221 - Digital input function upload
        static void MID0221Handler(string data)
        {
            string inputNo = data.Substring(22, 3);
            string status = data.Substring(27, 1);
            Console.WriteLine($"-> Dijital Giriş {inputNo}: {(status == "1" ? "AKTİF" : "PASİF")}");
        }
        #endregion

        #region Yardımcı: Gönder/Al
        static async Task SendAndReceive(NetworkStream stream, TcpClient client, string message)
        {
            byte[] outStream = Encoding.ASCII.GetBytes(message);
            stream.Write(outStream, 0, outStream.Length);
            stream.Flush();

            byte[] buffer = new byte[client.ReceiveBufferSize];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            Console.WriteLine("Alınan veri: " + data);

            // MID'ye göre yönlendirme
            if (data.Contains("0211")) MID0211Handler(data);
            else if (data.Contains("0217")) MID0217Handler(data);
            else if (data.Contains("0221")) MID0221Handler(data);
        }
        #endregion


        static void MID01(NetworkStream stream, TcpClient client)
        {

            #region MID0001 SEND

            Console.WriteLine("\nMID0001 Gönderiliyor...\n");
            string MID1 = "00200001            \0";
            byte[] outstream1 = Encoding.Default.GetBytes(MID1);
            stream.Write(outstream1, 0, outstream1.Length);
            stream.Flush();

            byte[] buffer = new byte[client.ReceiveBufferSize];

            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Alınan veri: " + data);

            #endregion

        }

        static void MID08(NetworkStream stream, TcpClient client)
        {

            #region MID0008 SEND

            Console.WriteLine("\nMID0008 Gönderiliyor...\n");
            //string MID2 = "006000080010        1201002310000000000000000000000000000001";
            string MID2 = "006000080010        1201002310000000000000000000000000000001";



            byte[] outstream2 = Encoding.Default.GetBytes(MID2);
            stream.Write(outstream2, 0, outstream2.Length);
            stream.Flush();

            byte[] outstream6 = Encoding.Default.GetBytes(MID2);
            stream.Write(outstream2, 0, outstream2.Length);
            stream.Flush();


            byte[] buffer = new byte[client.ReceiveBufferSize];

            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Alınan veri: " + data);

            Task.Delay(1000); // Çalıştırdıktan sonra data gelmemesi için 1 saniye bekliyoruz

            #endregion

            do
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("\n\n-> Alınan veri: " + data);

            } while (data.Substring(4, 4) != "1202");

            DataParse(data);

            Console.WriteLine("\n\n----------------------------------------------------------------------------------");



            TekrarVidaDinle(stream, client);

        }


        static void TekrarVidaDinle(NetworkStream stream, TcpClient client)
        {
            string data = "";

            Console.WriteLine("\n\n-> Vida atınız...");

            do
            {
                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("\n\n-> Alınan veri: " + data);

            } while (data.Substring(4, 4) != "1202");

            DataParse(data);

            Console.WriteLine("\n\n----------------------------------------------------------------------------------");

            TekrarVidaDinle(stream, client);
        }

        static void DataParse(string dataread)
        {


            // Create model
            AtlasCopcoDataDTO Model = new AtlasCopcoDataDTO();

            // Data parse
            var ParseMessageResult = MessageParser.ParseMessage(dataread);

            // Model fill

            if (ParseMessageResult.FirstOrDefault(c => c.ParameterId == "30208") != null)
                Model.ControllerName = ParseMessageResult.FirstOrDefault(c => c.ParameterId == "30208").Value;

            if (ParseMessageResult.FirstOrDefault(c => c.ParameterId == "30241") != null)
                Model.TorqueSts = ParseMessageResult.FirstOrDefault(c => c.ParameterId == "30241").Value;

            if (ParseMessageResult.FirstOrDefault(c => c.ParameterId == "30242") != null)
                Model.AngleSts = ParseMessageResult.FirstOrDefault(c => c.ParameterId == "30242").Value;

            if (ParseMessageResult.FirstOrDefault(c => c.ParameterId == "30200") != null)
                Model.TighteningId = ParseMessageResult.FirstOrDefault(c => c.ParameterId == "30200").Value;

            if (ParseMessageResult.FirstOrDefault(c => c.ParameterId == "30203") != null)
                Model.Time = ParseMessageResult.FirstOrDefault(c => c.ParameterId == "30203").Value;

            if (ParseMessageResult.FirstOrDefault(c => c.ParameterId == "30237") != null)
                if (decimal.TryParse(ParseMessageResult.FirstOrDefault(c => c.ParameterId == "30237").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal torque))
                    Model.Torque = torque.ToString();

            if (ParseMessageResult.FirstOrDefault(c => c.ParameterId == "30238") != null)
                if (decimal.TryParse(ParseMessageResult.FirstOrDefault(c => c.ParameterId == "30238").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal angle))
                    Model.Angle = angle.ToString();



            Console.WriteLine($"\n\nControllerName : {Model.ControllerName}");
            Console.WriteLine($"Time : {Model.Time}");
            Console.WriteLine($"TighteningId : {Model.TighteningId}");

            Console.WriteLine($"TorqueSts : {Model.TorqueSts}");
            Console.WriteLine($"Torque : {Model.Torque}");

            Console.WriteLine($"AngleSts : {Model.AngleSts}");
            Console.WriteLine($"Angle : {Model.Angle}");

        }


        // GetParameterPSet
        static void MID0010(NetworkStream stream, TcpClient client)
        {
            try
            {
                Console.WriteLine("\nMID0010 Gönderiliyor...");

                string MID0010 = $"002000100010         ";

                byte[] outstream = Encoding.ASCII.GetBytes(MID0010);
                stream.Write(outstream, 0, outstream.Length);
                stream.Flush();

                Console.WriteLine("Mesaj gönderildi: " + MID0010);

                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Alınan veri: " + data);

                // Parametre setlerini parse etme
                string parameterSets = data.Substring(23).Trim(); // İlk 23 karakteri atla, parametre setleri burada başlıyor

                // Parametre setlerini listele
                Dictionary<int, string> psetMap = new Dictionary<int, string>();
                for (int i = 0; i < parameterSets.Length; i += 3)
                {
                    if (i + 3 <= parameterSets.Length)
                    {
                        string psetId = parameterSets.Substring(i, 3);
                        int simplifiedPsetId = int.Parse(psetId);
                        psetMap[simplifiedPsetId] = psetId;

                        Console.WriteLine($"Pset ID: {simplifiedPsetId}");
                    }
                    else
                    {
                        Console.WriteLine("Veri uzunluğu yeterli değil, işlem yapılamaz.");
                    }
                }

                Console.WriteLine("\nBir parametre seti seçin :");
                if (int.TryParse(Console.ReadLine(), out int userChoice) && psetMap.ContainsKey(userChoice))
                {
                    string selectedPsetID = psetMap[userChoice];
                    Console.WriteLine($"Seçilen PSET: {userChoice} (Gerçek ID: {selectedPsetID})");

                    MID0018(stream, client, int.Parse(selectedPsetID));
                }
                else
                {
                    Console.WriteLine("Geçersiz seçim. Lütfen geçerli bir Pset ID'si girin.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
            }
        }


        static void MID0018(NetworkStream stream, TcpClient client, int parameterSetID)
        {
            try
            {
                Console.WriteLine("\nMID0018 Gönderiliyor...");

                string parameterSetIDStr = parameterSetID.ToString("D3");
                string MID0018 = $"00230018 0010   00  {parameterSetIDStr}\0";

                byte[] outstream = Encoding.ASCII.GetBytes(MID0018);
                stream.Write(outstream, 0, outstream.Length);
                stream.Flush();

                Console.WriteLine("Mesaj gönderildi: " + MID0018);

                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Alınan veri: " + data);

                if (data.Substring(4, 4) == "0005")
                {
                    Console.WriteLine($"Pset ayarı kabul edildi. Seçilen PSET: {parameterSetID}");
                }
                else if (data.Substring(4, 4) == "0004")
                {
                    Console.WriteLine("Komut hatası: Parametre seti ayarlanamadı.");
                }
                else
                {
                    Console.WriteLine("Bilinmeyen cevap alındı.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
            }
        }



    }
}
