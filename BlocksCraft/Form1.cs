﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

using SuperMap.Data;
using SuperMap.Realspace;
using SuperMap.UI;

using BlocksCraft.ModelIncubation;

namespace BlocksCraft
{
    public partial class Form1 : Form
    {
        Workspace ws;
        SceneControl scon;
        public Form1()
        {
            InitializeComponent();
            init();
        }

        void init()
        {
            scon = new SceneControl();
            scon.Dock = DockStyle.Fill;
            this.Controls.Add(scon);
            string wsPath = @"D:\模型找正面\ws.smwu";
            ws = new Workspace();
            WorkspaceConnectionInfo wsCon = new WorkspaceConnectionInfo()
            {
                Server = wsPath,
                Type = WorkspaceType.SMWU
            };

            ws.Open(wsCon);

            scon.Scene.Workspace = ws;
            scon.Scene.Open(ws.Scenes[0]);
            scon.Scene.Sun.IsVisible = true;
            scon.Scene.EnsureVisible(scon.Scene.Layers[0]);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.scon.Scene.TrackingLayer.Clear();
            foreach (Datasource datasource in ws.Datasources)
            {
                foreach (Dataset dataset in datasource.Datasets)
                {
                    switch (dataset.Type)
                    {
                        case DatasetType.CAD:
                            Phineas p = new Phineas()
                            {
                                dv = dataset as DatasetVector,
                                scene = scon.Scene
                            };
                            new Thread(p.run).Start();
                            break;
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.scon.Scene.TrackingLayer.Clear();
            foreach (Datasource datasource in ws.Datasources)
            {
                foreach (Dataset dataset in datasource.Datasets)
                {
                    switch (dataset.Type)
                    {
                        case DatasetType.CAD:
                            Phineas p = new Phineas()
                            {
                                dv = dataset as DatasetVector,
                                scene = scon.Scene
                            };
                            new Thread(p.ClipTest).Start();
                            break;
                    }
                }
            }
        }
    }
    class Phineas
    {
        public DatasetVector dv;
        public Scene scene;
        Random R = new Random();

        public void run()
        {
            Recordset rc = dv.GetRecordset(false, CursorType.Dynamic);
            Dictionary<int, Feature> feas = rc.GetAllFeatures();

            foreach (KeyValuePair<int, Feature> item in feas)
            {
                GeoModel gm = item.Value.GetGeometry() as GeoModel;
                Console.WriteLine("==" + gm.Position + "==");

                //GeoModel model = new ModelIncubation.CuboidModel(10, 10, 10);
                Point3Ds p3ds = new Point3Ds();
                for (int i = 0; i < 7; i++)
			    {
                    double seta = 2 * Math.PI * i / 7;
                    p3ds.Add(new Point3D(Math.Sin(seta)*10, Math.Cos(seta)*10, 0));
			    }
                GeoModel model = new ModelIncubation.PrismModel(p3ds, 30);

                //临时处理，未知原因导致Position.Z属性设置无效，手动偏移模型实体
                model.OffsetModel(new Point3D(0, 0, 1650));
                model.Position = gm.Position;
                model.MergeMeshs();
                Console.WriteLine("");

                model.Position = gm.Position;
                model.ComputeBoundingBox();
                scene.TrackingLayer.Add(model, model.Position.ToString());
                scene.Refresh();
                Thread.Sleep(1000);

                break;
            }
        }

        public void ClipTest()
        {
            Recordset rc = dv.GetRecordset(false, CursorType.Dynamic);
            Dictionary<int, Feature> feas = rc.GetAllFeatures();

            foreach (KeyValuePair<int, Feature> item in feas)
            {
                GeoModel gm = item.Value.GetGeometry() as GeoModel;
                Console.WriteLine("==" + gm.Position + "==");

                //GeoModel model = new ModelIncubation.CuboidModel(10, 10, 10);
                Point3Ds p3ds = new Point3Ds();
                for (int i = 0; i < 7; i++)
                {
                    double seta = 2 * Math.PI * i / 7;
                    p3ds.Add(new Point3D(Math.Sin(seta) * 10, Math.Cos(seta) * 10, 0));
                }
                GeoModel model = new ModelIncubation.PrismModel(p3ds, 30);
                //临时处理，未知原因导致Position.Z属性设置无效，手动偏移模型实体
                model.OffsetModel(new Point3D(0, 0, 1650));
                model.Position = gm.Position;
                model.MergeMeshs();
                Console.WriteLine("");
                #region 模型切割测试
                GeoModelEx.Surface s = new GeoModelEx.Surface(0.1, 0.5, 1, -1672);
                model.ClipModel(s);
                #endregion
                model.Position = gm.Position;
                model.ComputeBoundingBox();
                scene.TrackingLayer.Add(model, model.Position.ToString());
                scene.Refresh();
                Thread.Sleep(1000);

                break;
            }
        }
    }
}