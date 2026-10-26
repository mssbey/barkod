using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

namespace EtiketAtolyesi {
 public class App : Form {
  static readonly string[] Patterns = ("212222 222122 222221 121223 121322 131222 122213 122312 132212 221213 221312 231212 112232 122132 122231 113222 123122 123221 223211 221132 221231 213212 223112 312131 311222 321122 321221 312212 322112 322211 212123 212321 232121 111323 131123 131321 112313 132113 132311 211313 231113 231311 112133 112331 132131 113123 113321 133121 313121 211331 231131 213113 213311 213131 311123 311321 331121 312113 312311 332111 314111 221411 431111 111224 111422 121124 121421 141122 141221 112214 112412 122114 122411 142112 142211 241211 221114 413111 241112 134111 111242 121142 121241 114212 124112 124211 411212 421112 421211 212141 214121 412121 111143 111341 131141 114113 114311 411113 411311 113141 114131 311141 411131 211412 211214 211232 2331112").Split(' ');
  TextBox title = new TextBox(), body = new TextBox(), barcode = new TextBox(), footer = new TextBox();
  ComboBox printer = new ComboBox(), size = new ComboBox();
  NumericUpDown copies = new NumericUpDown(), fontSize = new NumericUpDown();
  CheckBox useBarcode = new CheckBox(), bold = new CheckBox();
  PictureBox preview = new PictureBox();
  Label status = new Label();
  Button print = new Button();
  bool ready;
  public App() {
   Text = "Etiket Atölyesi · Xprinter XP-490B"; Width = 1050; Height = 760;
   MinimumSize = new Size(920,710); StartPosition = FormStartPosition.CenterScreen;
   BackColor = Color.FromArgb(240,244,248); Font = new Font("Segoe UI",10);
   var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount=2, Padding=new Padding(22) };
   root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,410)); root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100)); Controls.Add(root);
   var form = new FlowLayoutPanel { Dock=DockStyle.Fill, FlowDirection=FlowDirection.TopDown, WrapContents=false, AutoScroll=true };
   root.Controls.Add(form,0,0);
   AddLabel(form,"ETİKET ATÖLYESİ",19,FontStyle.Bold);
   AddLabel(form,"Yazınızı hazırlayın, önizleyin, yazdırın.",10,FontStyle.Regular);
   AddLabel(form,"Yazıcı",10,FontStyle.Bold); printer.Width=375; printer.DropDownStyle=ComboBoxStyle.DropDownList; form.Controls.Add(printer);
   var refresh=new Button { Text="Yazıcı listesini yenile",Width=375,Height=30 }; refresh.Click+=(s,e)=>LoadPrinters(); form.Controls.Add(refresh);
   AddLabel(form,"Etiket ölçüsü (genişlik × yükseklik)",10,FontStyle.Bold);
   size.Width=375; size.DropDownStyle=ComboBoxStyle.DropDownList; size.Items.AddRange(new object[]{"50 × 30 mm — yatay","30 × 50 mm — dikey"}); size.SelectedIndex=0; form.Controls.Add(size);
   AddInput(form,"Başlık (isteğe bağlı)",title,false); title.Text="ÜRÜN ETİKETİ";
   AddInput(form,"Yazı / açıklama (isteğe bağlı)",body,true); body.Text="Buraya istediğiniz yazıyı yazın.";
   var opts=new FlowLayoutPanel {Width=380,Height=35};
   opts.Controls.Add(new Label {Text="Yazı boyutu",AutoSize=true,Padding=new Padding(0,5,0,0)});
   fontSize.Minimum=7; fontSize.Maximum=36; fontSize.Value=12; fontSize.Width=60; opts.Controls.Add(fontSize);
   bold.Text="Kalın"; bold.Width=90; opts.Controls.Add(bold); form.Controls.Add(opts);
   useBarcode.Text="Barkod ekle (Code 128)"; useBarcode.Width=375; useBarcode.Checked=true; form.Controls.Add(useBarcode);
   barcode.Width=375; barcode.Text="8691234567890"; form.Controls.Add(barcode);
   AddInput(form,"Alt satır / fiyat (isteğe bağlı)",footer,false);
   var actions=new FlowLayoutPanel {Width=380,Height=44,Margin=new Padding(0,14,0,0)};
   actions.Controls.Add(new Label{Text="Adet",AutoSize=true,Padding=new Padding(0,8,0,0)});
   copies.Minimum=1; copies.Maximum=500; copies.Value=1; copies.Width=65; actions.Controls.Add(copies);
   print.Text="Yazdır"; print.Width=220; print.Height=36; print.BackColor=Color.FromArgb(15,100,95); print.ForeColor=Color.White; print.FlatStyle=FlatStyle.Flat; print.Click+=(s,e)=>PrintLabels(); actions.Controls.Add(print); form.Controls.Add(actions);
   var right=new TableLayoutPanel { Dock=DockStyle.Fill,RowCount=4,ColumnCount=1,Padding=new Padding(20,0,0,0) };
   right.RowStyles.Add(new RowStyle(SizeType.Absolute,45)); right.RowStyles.Add(new RowStyle(SizeType.Percent,100)); right.RowStyles.Add(new RowStyle(SizeType.Absolute,85)); right.RowStyles.Add(new RowStyle(SizeType.Absolute,100)); root.Controls.Add(right,1,0);
   right.Controls.Add(new Label{Text="CANLI ÖNİZLEME",Font=new Font("Segoe UI",12,FontStyle.Bold),Dock=DockStyle.Fill},0,0);
   preview.Dock=DockStyle.Fill; preview.SizeMode=PictureBoxSizeMode.Zoom; preview.BackColor=Color.FromArgb(215,225,231); right.Controls.Add(preview,0,1);
   status.Dock=DockStyle.Fill; status.Padding=new Padding(0,12,0,0); right.Controls.Add(status,0,2);
   right.Controls.Add(new Label{Text="İlk baskıda 1 adet deneyin.\nYazıcı tercihlerinde aynı etiket ölçüsünü seçin.\nÖnizleme ekranda büyütülmüştür. Baskı gerçek ölçüdedir.\nBarkodlar en az 2 yazıcı noktası kalınlığında hazırlanır.",Dock=DockStyle.Fill,ForeColor=Color.FromArgb(70,85,95)},0,3);
   foreach(var t in new[]{title,body,barcode,footer}) t.TextChanged+=(s,e)=>UpdatePreview();
   size.SelectedIndexChanged+=(s,e)=>UpdatePreview(); fontSize.ValueChanged+=(s,e)=>UpdatePreview(); bold.CheckedChanged+=(s,e)=>UpdatePreview(); useBarcode.CheckedChanged+=(s,e)=>{barcode.Enabled=useBarcode.Checked;UpdatePreview();};
   ready=true; LoadPrinters(); UpdatePreview();
  }
  static void AddLabel(Control p,string text,int pt,FontStyle style) {p.Controls.Add(new Label{Text=text,AutoSize=true,MaximumSize=new Size(375,0),Font=new Font("Segoe UI",pt,style),Margin=new Padding(0,8,0,5)});}
  static void AddInput(Control p,string label,TextBox t,bool multi) {AddLabel(p,label,10,FontStyle.Bold);t.Width=375;t.Multiline=multi;if(multi){t.Height=70;t.ScrollBars=ScrollBars.Vertical;}p.Controls.Add(t);}
  void LoadPrinters() {
   string old=printer.SelectedItem as string; printer.Items.Clear();
   try { foreach(string p in PrinterSettings.InstalledPrinters) printer.Items.Add(p);
    if(old!=null && printer.Items.Contains(old)) printer.SelectedItem=old;
    else {foreach(string p in printer.Items) if(p.IndexOf("490",StringComparison.OrdinalIgnoreCase)>=0) {printer.SelectedItem=p;break;}}
    if(printer.SelectedIndex<0 && printer.Items.Count>0) printer.SelectedIndex=0;
   } catch(Exception ex) {MessageBox.Show("Yazıcı listesi alınamadı: "+ex.Message);}
  }
  public static List<int> Encode(string value) {
   if(string.IsNullOrEmpty(value)) throw new Exception("Barkod değerini yazın veya barkod seçeneğini kapatın.");
   foreach(char c in value) if(c<32 || c>126) throw new Exception("Barkodda yalnızca İngilizce harf, rakam ve standart işaret kullanın. Türkçe metni yazı alanına yazabilirsiniz.");
   var codes=new List<int>(); bool modeC=false; codes.Add(104); int i=0;
   while(i<value.Length) {
    int run=0;while(i+run<value.Length && value[i+run]>='0' && value[i+run]<='9') run++;
    if(!modeC && run>=4) { if(run%2==1) {codes.Add(value[i]-32);i++;} codes.Add(99);modeC=true;continue; }
    if(modeC) {if(run>=2){codes.Add((value[i]-'0')*10+value[i+1]-'0');i+=2;continue;} codes.Add(100);modeC=false;continue;}
    codes.Add(value[i]-32);i++;
   }
   int check=codes[0];for(int k=1;k<codes.Count;k++) check+=codes[k]*k;
   codes.Add(check%103);codes.Add(106);return codes;
  }
  public static Bitmap Render(int wmm,int hmm,string heading,string text,string code,string bottom,bool bar,int points,bool heavy) {
   int w=(int)Math.Round(wmm*203/25.4),h=(int)Math.Round(hmm*203/25.4);
   var bmp=new Bitmap(w,h);bmp.SetResolution(203,203);
   try {using(var g=Graphics.FromImage(bmp)) {
    g.Clear(Color.White);g.TextRenderingHint=TextRenderingHint.SingleBitPerPixelGridFit;
    int pad=16, y=pad, available=w-pad*2;
    var sf=new StringFormat {Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center};
    if(!string.IsNullOrWhiteSpace(heading)) {using(var f=new Font("Arial",26,FontStyle.Bold,GraphicsUnit.Pixel)) {
     if(g.MeasureString(heading,f).Width>available) throw new Exception("Başlık etikete sığmıyor. Başlığı kısaltın.");
     g.DrawString(heading,f,Brushes.Black,new RectangleF(pad,y,available,34),sf);y+=36;
    }}
    int bottomHeight=string.IsNullOrWhiteSpace(bottom)?0:30;
    int barHeight=bar?85:0;
    int textHeight=h-pad-y-bottomHeight-barHeight;
    if(!string.IsNullOrWhiteSpace(text)) {
     using(var f=new Font("Arial",points*203f/72f,heavy?FontStyle.Bold:FontStyle.Regular,GraphicsUnit.Pixel)) {
      var measure=g.MeasureString(text,f,available,sf);
      if(textHeight<1 || measure.Height>textHeight) throw new Exception("Yazı etikete sığmıyor. Yazıyı kısaltın, boyutunu küçültün veya barkodu kapatın.");
      g.DrawString(text,f,Brushes.Black,new RectangleF(pad,y,available,textHeight),sf);
     }
    }
    y=h-pad-bottomHeight-barHeight;
    if(bar) {
     var codes=Encode(code);int modules=20;
     foreach(int c in codes) foreach(char n in Patterns[c]) modules+=n-'0';
     int unit=available/modules;
     if(unit<2) throw new Exception("Barkod bu etikete güvenilir biçimde sığmıyor. Değeri kısaltın veya 50 × 30 mm seçin. Sayısal kodlar daha az yer kaplar.");
     int x=(w-modules*unit)/2+10*unit;
     foreach(int c in codes) {bool black=true;foreach(char n in Patterns[c]){int dx=(n-'0')*unit;if(black)g.FillRectangle(Brushes.Black,x,y+4,dx,52);x+=dx;black=!black;}}
     using(var f=new Font("Arial",18,GraphicsUnit.Pixel)) g.DrawString(code,f,Brushes.Black,new RectangleF(pad,y+57,available,25),sf);
     y+=barHeight;
    }
    if(bottomHeight>0) using(var f=new Font("Arial",22,FontStyle.Bold,GraphicsUnit.Pixel)) {
     if(g.MeasureString(bottom,f).Width>available) throw new Exception("Alt satır etikete sığmıyor. Metni kısaltın.");
     g.DrawString(bottom,f,Brushes.Black,new RectangleF(pad,y,available,bottomHeight),sf);
    }
    sf.Dispose();
   }return bmp;} catch {bmp.Dispose();throw;}
  }
  Bitmap Current() {return Render(size.SelectedIndex==0?50:30,size.SelectedIndex==0?30:50,title.Text,body.Text,barcode.Text,footer.Text,useBarcode.Checked,(int)fontSize.Value,bold.Checked);}
  void UpdatePreview() {if(!ready)return;try{var img=Current();var old=preview.Image;preview.Image=img;if(old!=null)old.Dispose();status.ForeColor=Color.FromArgb(15,100,95);status.Text="Etiket hazır. "+(size.SelectedIndex==0?"50 × 30":"30 × 50")+" mm · 203 dpi";print.Enabled=true;}catch(Exception ex){var old=preview.Image;preview.Image=null;if(old!=null)old.Dispose();status.ForeColor=Color.Firebrick;status.Text=ex.Message;print.Enabled=false;}}
  void PrintLabels() {
   if(printer.SelectedItem==null) {MessageBox.Show("Önce XP-490B Windows sürücüsünü kurun, ardından yazıcı listesini yenileyin.");return;}
   try {using(var image=Current()) using(var doc=new PrintDocument()) {
    doc.DocumentName="Etiket Atölyesi";doc.PrinterSettings.PrinterName=(string)printer.SelectedItem;
    if(!doc.PrinterSettings.IsValid)throw new Exception("Seçilen yazıcı kullanılamıyor.");
    int wmm=size.SelectedIndex==0?50:30,hmm=size.SelectedIndex==0?30:50;
    doc.DefaultPageSettings.PaperSize=new PaperSize("Etiket "+wmm+"x"+hmm,(int)Math.Round(wmm/25.4*100),(int)Math.Round(hmm/25.4*100));
    doc.DefaultPageSettings.Margins=new Margins(0,0,0,0);doc.DefaultPageSettings.Landscape=false;
    doc.OriginAtMargins=false;int remaining=(int)copies.Value;
    doc.PrintPage+=(s,e)=>{
     e.Graphics.TranslateTransform(-e.PageSettings.HardMarginX,-e.PageSettings.HardMarginY);
     e.Graphics.InterpolationMode=InterpolationMode.NearestNeighbor;e.Graphics.PixelOffsetMode=PixelOffsetMode.Half;
     e.Graphics.DrawImage(image,new RectangleF(0,0,wmm/25.4f*100,hmm/25.4f*100));
     remaining--;e.HasMorePages=remaining>0;
    };
    doc.Print();status.Text=copies.Value+" etiket Windows yazdırma kuyruğuna gönderildi. Fiziksel çıktıyı kontrol edin.";
   }}catch(Exception ex){MessageBox.Show(ex.Message,"Yazdırma",MessageBoxButtons.OK,MessageBoxIcon.Error);}
  }
  public static void SelfTest(string folder) {
   using(var app=new App()) {if(!app.print.Enabled || app.preview.Image==null)throw new Exception("İlk etiket önizlemesi oluşturulamadı: "+app.status.Text);}
   if(Patterns.Length!=107)throw new Exception("Barkod tablosu eksik.");
   for(int k=0;k<107;k++){int sum=0;foreach(char c in Patterns[k])sum+=c-'0';if(sum!=(k==106?13:11))throw new Exception("Barkod deseni hatalı.");}
   var abc=Encode("ABC");if(string.Join(",",abc.ToArray())!="104,33,34,35,1,106")throw new Exception("Code128 kontrol toplamı hatalı.");
   using(var b=Render(50,30,"ÜRÜN ETİKETİ","Yüzücü gözlüğü","8691234567890","250 TL",true,10,false)) b.Save(Path.Combine(folder,"Ornek-Etiket.png"));
   using(var b=Render(30,50,"MERHABA","Türkçe yazı\nÇĞİÖŞÜ\nçğıöşü","","",false,12,true)) b.Save(Path.Combine(folder,"Ornek-Dikey.png"));
   bool failed=false;try{Encode("ş");}catch{failed=true;}if(!failed)throw new Exception("Geçersiz kod reddedilmedi.");
   failed=false;try{using(var b=Render(50,30,"","",new string('A',70),"",true,12,false)) {}}catch{failed=true;}if(!failed)throw new Exception("Taşma reddedilmedi.");
   Console.WriteLine("Tamam: Code128 desenleri, kontrol toplami, Turkce render, yatay/dikey etiket, gecersiz kod ve tasma kontrolleri.");
  }
 }
}
