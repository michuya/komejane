using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;

namespace KomejaneUserTest
{
  class Class1
  {
    [STAThread]
    static void Main(string[] args)
    {
      Komejane.Komejane komejane = new Komejane.Komejane();

      komejane.Initialize();

      GenComment(komejane);

      komejane.Run();

      StopGenComment();
    }

    static bool CanceledGenComment = false;
    static void StopGenComment() { CanceledGenComment = true; }
    static async void GenComment(Komejane.Komejane komejane)
    {
      await Task.Delay(5000); // 5秒待機
      await Task.Run(() =>
      {
        var rand = new Random();
        int cid = 1;

        while (!CanceledGenComment)
        {
          var comment = new Komejane.CommentData(cid++, Faker.Lorem.Sentence());
          if (rand.Next(7) == 0) comment.CharaName = Faker.Name.First();
          Console.WriteLine("GenComment => " + comment);

          komejane.AddComment(comment);

          Thread.Sleep(500 + rand.Next(3000));
        }
      });
    }
  }
}
