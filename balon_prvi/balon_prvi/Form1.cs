using System.CodeDom.Compiler;
using System.Drawing.Drawing2D;
using System.Reflection.Emit;

/*
Siguran sam da ovo nije kako smo ucili ili kako smo trebali ovo da uradimo ali ja sam to tako uradio
Ovo resenje je inspirisano pomocu MSDN dokumentacije so System.Drawing 
Originalno sam hteo da koristim jednacinu elipse iz analisticke geometrije ali to mi je davalo podatak da li je klik na ivici elipse a ne u njoj
Moglo bi to da se resi koncentricnim elipsama ali je previse sporo pa smesno izgleda (probao sam)

Pokusao sam da dokumentujem sta radi sta i da otklonim sto vise bagova ali iz nekog nepoznatog razloga baloni pocnu da trepere cak i kad zaustavim izvrsenje koda debugerom
Predpostavljam da je problem do samog interfejsa izmedju programa i grafickog drajvera ali da to potvrdim morao bih da imam pristup samom drajveru a posto nisam toliko lud
A imam nVida graficku nisam u mogucnosti da potvrdim tu teoriju (izgleda da je problem do samog drajvera ili gpu posto ovaj isti program kompajliran za arm linux (debian 32bit) radi)
Naravno problem moze biti i u System.Drawing/Drawing2D ali ni to ne mogu lagano da potvrdim pa cu predpostaviti da problem nije u mom programu
Sobzirom da se problem javlja i kada je moj kod zaustavljen

Srecno onome ko pokusa ono da razume :)
*/


namespace balon_prvi
{
    public partial class Form1 : Form
    {
        Graphics g;


        int[] x = new int[20];
        int[] y = new int[20];
        int[] a = new int[20];
        bool[] probuse = new bool[20];
        bool brisano;
        bool[] obrisan = new bool[20];
        Pen[] boja = new Pen[20];
        Region[] hitboksovi = new Region[20];
        System.Drawing.Drawing2D.GraphicsPath[] magija = new System.Drawing.Drawing2D.GraphicsPath[20];

        int R, G, B;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            //Proveravamo da li je lokacija klika u bilo kom od regiona koji cine baloni
            for (int i = 0; i < 20; i++)
            {
                if (hitboksovi[i].IsVisible(e.Location))    
                {
                    probuse[i] = true;
                }
            }

        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            //No no korisnik ne sme da menja velicinu ili program ide bum
            this.Size = new Size(800, 600);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            g = this.CreateGraphics();  //MSDN dokumentacija kaze da je ovo pozeljno da bude ovde
            this.Size = new Size(800, 600);//MORAMO BITI SIGURNI DA JE VELICINA KOREKTNA
            brisano = true;

            for (int i = 0; i < 20; i++)    // Kreiramo sve parametre za balone
            {
                obrisan[i] = false;  
                probuse[i] = false;
                Random r = new Random();

                x[i] = r.Next(800);
                y[i] = r.Next(600);
                a[i] = r.Next(5, 35);

                R = r.Next(0, 255);
                G = r.Next(0, 255);
                B = r.Next(0, 255);

                boja[i] = new Pen(Color.Transparent);
                boja[i].Color = Color.FromArgb(R, G, B);
                boja[i].Width = 1;


                //Kreiramo hitbox za balone tako sto prvo kreiramo putanju koju cini cvor pa toj putanji dodamo elips i na kraju tu jednu ogromnu putanj(povrsinu kada se popuni)
                //pretvaramo u skup kordinata(odnosno region) kako bi mogli da proverimo da li se neki klik nalazi u njemu
                byte[] tip_tacke = new byte[3]; 
                tip_tacke[0] = (byte)1;
                tip_tacke[1] = (byte)1;
                tip_tacke[2] = (byte)1;
                Point[] tacke = new Point[3];
                tacke[0] = new Point(x[i], y[i]);
                tacke[1] = new Point(x[i] - a[i], y[i] + a[i]);
                tacke[2] = new Point(x[i] + a[i], y[i] + a[i]);
                magija[i] = new GraphicsPath(tacke, tip_tacke);
                magija[i].AddEllipse(x[i] - 3 * a[i], y[i] - 8 * a[i], 6 * a[i], 8 * a[i]);
                hitboksovi[i] = new Region(magija[i]);
            }
            timer1.Enabled = true;  //Ukljucujemo tajmer i time zapocinjemo zabavu
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            g.Dispose(); //MSDN preporucuje da se ovo stavi ovde (nije neophodno zato sto postoji garbage collector ali je pozeljno)
            for(int i = 0; i < 20; i++)// MSDN preporucuje da se ovo stavi pa sam stavio
            {
                hitboksovi[i].Dispose();
                magija[i].Dispose();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        { 
            for (int i = 0; i < 20; i++)
            {
                if (i == 19 && brisano)    // Ako smo sve opet iscrtali u boji mozemo da izdjemo iz ciklusa brisanja
                {
                    brisano = false;
                }

                if (!probuse[i])    //Proveravam da li treba da ga crtam ili brisem
                {
                    if (brisano)//Ovo celo sa brisanje je uvedeno zato sto bi u suprotnom blicali
                    {
                        //Crtam elipsu
                        g.DrawEllipse(Pens.White, x[i] - 3 * a[i], y[i] - 8 * a[i], 6 * a[i], 8 * a[i]);
                        g.FillEllipse(boja[i].Brush, x[i] - 3 * a[i], y[i] - 8 * a[i], 6 * a[i], 8 * a[i]);

                        //Ctam cvor
                        byte[] tip_tacke = new byte[3]; //Ovo indikuje da li su linije, isprekidane linije itd
                        tip_tacke[0] = (byte)1;//1 znaci da su linije
                        tip_tacke[1] = (byte)1;
                        tip_tacke[2] = (byte)1;
                        Point[] tacke = new Point[3];//Ovo predstavlja tri coska trougla
                        tacke[0] = new Point(x[i], y[i]);
                        tacke[1] = new Point(x[i] - a[i], y[i] + a[i]);
                        tacke[2] = new Point(x[i] + a[i], y[i] + a[i]);
                        GraphicsPath p = new GraphicsPath(tacke, tip_tacke);//Ovo je trougao
                        g.FillPath(boja[i].Brush, p);//Ovo izcrtava torugao

                        Point[] tacke2 = new Point[2];
                        tacke2[0] = new Point(x[i], y[i] + a[i]);
                        tacke2[1] = new Point(x[i] - a[i], y[i] + 2 * a[i]);
                        g.DrawLine(Pens.Cyan, tacke2[0], tacke2[1]);

                        Point[] tacke3 = new Point[2];
                        tacke3[0] = new Point(x[i] - a[i], y[i] + 2 * a[i]);
                        tacke3[1] = new Point(x[i] + a[i], y[i] + 3 * a[i]);
                        g.DrawLine(Pens.Cyan, tacke3[0], tacke3[1]);

                        Point[] tacke4 = new Point[2];
                        tacke4[0] = new Point(x[i] + a[i], y[i] + 3 * a[i]);
                        tacke4[1] = new Point(x[i] - a[i], y[i] + 4 * a[i]);
                        g.DrawLine(Pens.Cyan, tacke4[0], tacke4[1]);

                        Point[] tacke5 = new Point[2];
                        tacke5[0] = new Point(x[i] - a[i], y[i] + 4 * a[i]);
                        tacke5[1] = new Point(x[i] + a[i], y[i] + 5 * a[i]);
                        g.DrawLine(Pens.Cyan, tacke5[0], tacke5[1]);
                    }

                }
                else //Ovde ga brisem
                {
                    if (!obrisan[i]) //Proveravam da li vec nije obrisan posto nema smisla da ga brisem dva puta
                    {
                        //Crtam elipsu
                        g.DrawEllipse(Pens.Black, x[i] - 3 * a[i], y[i] - 8 * a[i], 6 * a[i], 8 * a[i]);
                        g.FillEllipse(Pens.Black.Brush, x[i] - 3 * a[i], y[i] - 8 * a[i], 6 * a[i], 8 * a[i]);

                        //Ctam cvor
                        byte[] tip_tacke = new byte[3]; //Ovo indikuje da li su linije, isprekidane linije itd
                        tip_tacke[0] = (byte)1;//1 znaci da su linije
                        tip_tacke[1] = (byte)1;
                        tip_tacke[2] = (byte)1;
                        Point[] tacke = new Point[3];//Ovo predstavlja tri coska trougla
                        tacke[0] = new Point(x[i], y[i]);
                        tacke[1] = new Point(x[i] - a[i], y[i] + a[i]);
                        tacke[2] = new Point(x[i] + a[i], y[i] + a[i]);
                        GraphicsPath p = new GraphicsPath(tacke, tip_tacke);//Ovo je trougao
                        g.FillPath(Pens.Black.Brush, p);//Ovo izcrtava torugao

                        Point[] tacke2 = new Point[2];
                        tacke2[0] = new Point(x[i], y[i] + a[i]);
                        tacke2[1] = new Point(x[i] - a[i], y[i] + 2 * a[i]);
                        g.DrawLine(Pens.Black, tacke2[0], tacke2[1]);

                        Point[] tacke3 = new Point[2];
                        tacke3[0] = new Point(x[i] - a[i], y[i] + 2 * a[i]);
                        tacke3[1] = new Point(x[i] + a[i], y[i] + 3 * a[i]);
                        g.DrawLine(Pens.Black, tacke3[0], tacke3[1]);

                        Point[] tacke4 = new Point[2];
                        tacke4[0] = new Point(x[i] + a[i], y[i] + 3 * a[i]);
                        tacke4[1] = new Point(x[i] - a[i], y[i] + 4 * a[i]);
                        g.DrawLine(Pens.Black, tacke4[0], tacke4[1]);

                        Point[] tacke5 = new Point[2];
                        tacke5[0] = new Point(x[i] - a[i], y[i] + 4 * a[i]);
                        tacke5[1] = new Point(x[i] + a[i], y[i] + 5 * a[i]);
                        g.DrawLine(Pens.Black, tacke5[0], tacke5[1]);

                        obrisan[i] = true;  //Postavljamo flag koji nam govori da smo ovaj balon obrisali
                        brisano = true; //Indikujemo da zelimo ponovni prolaz crtanje
                        break; //OVO JE MILION PUTA PAMETNIJE NEGO i=0 !!!!
                    }
                }
            }
        }
    }
}
