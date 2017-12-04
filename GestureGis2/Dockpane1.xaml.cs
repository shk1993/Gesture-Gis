using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using System.Windows.Shapes;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Editing;

namespace GestureGis2
{
    /// <summary>
    /// Interaction logic for Dockpane1View.xaml
    /// </summary>
    public partial class Dockpane1View : UserControl
    {
        Point currentPoint = new Point();
        //this attributeIndex value is used to make sure that all the ids are unique
        int attributeIndex = 0;
        List<Point> gesture = new List<Point>();

        public Dockpane1View()
        {
            InitializeComponent();
        }

        private void sketchPad_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line line = new Line();
                gesture.Add(e.GetPosition(this));
                line.Stroke = SystemColors.ControlDarkDarkBrush;
                line.X1 = currentPoint.X;
                line.Y1 = currentPoint.Y;
                line.X2 = e.GetPosition(this).X;
                line.Y2 = e.GetPosition(this).Y;

                currentPoint = e.GetPosition(this);

                sketchPad.Children.Add(line);
            }
        }

        private void sketchPad_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                gesture.Add(e.GetPosition(this));
                currentPoint = e.GetPosition(this);
            }

        }

        protected async void Button_Click(object sender, RoutedEventArgs e)
        {
            gesture = new List<Point>();
            sketchPad.Children.Clear();

            try
            {
                BasicFeatureLayer layer = null;
                List<Inspector> inspList = new List<Inspector>();
                Inspector inspector = null;
                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    //find selected layer
                    if (MapView.Active.GetSelectedLayers().Count == 0)
                    {
                        //MessageBox.Show("Select a feature class from the Content 'Table of Content' first.");
                        return;
                    }
                    layer = MapView.Active.GetSelectedLayers()[0] as BasicFeatureLayer;
                    
                    // get selected features from the map
                    var features = layer.GetSelection();
                    if (features.GetCount() == 0)
                    {
                        return;
                        /*ToDo : add error msg: no feature selected*/
                    }

                    // get ids of all the selected features
                    var featOids = features.GetObjectIDs();
                    if (featOids.Count == 0)
                    {/* ToDo : something is wrong*/}
                    
                    // adding the inspectors to a list so that separate id values can be assigned later
                    for (int i = 0; i < featOids.Count; i++)
                    {
                        var insp = new Inspector();
                        insp.Load(layer, featOids.ElementAt(i));
                        inspList.Add(insp);
                    }
                    inspector = new Inspector();
                    inspector.Load(layer, featOids);

                });
                if (layer == null) { }
                //MessageBox.Show("Unable to find a feature class at the first layer of the active map");
                else
                {
                    //update the attributes of those features

                    // make sure tha attribute exists
                    ArcGIS.Desktop.Editing.Attributes.Attribute att = inspector.FirstOrDefault(a => a.FieldName == "UID");
                    if (att == null)
                    {
                        // if the attribute doesn't exist we create a new field
                        var dataSource = await GetDataSource(layer);
                        //MessageBox.Show($@"{dataSource} was found ... adding a new Field");
                        await
                            ExecuteAddFieldTool(layer, new KeyValuePair<string, string>("UID", "uniqueId"), "Text", 50);
                    }

                    await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                    {
                        // we add values of ids to the selected features
                        for(int i = 0; i < inspList.Count; i++)
                        {
                            if(inspList.ElementAt(i)["UID"]==null || (String)inspList.ElementAt(i)["UID"] == String.Empty)
                            {
                                // putting a random string now, this should be replaced by the tag user puts in after the recognition part is done.
                                inspList.ElementAt(i)["UID"] = "newAtr" + attributeIndex++;
                                var op = new EditOperation();
                                op.Name = "Update";
                                op.SelectModifiedFeatures = true;
                                op.SelectNewFeatures = false;
                                op.Modify(inspList.ElementAt(i));
                                op.Execute();
                            }
                            
                        }
                       /* var att2 = insp.Select(a => a.FieldName == "UID");
                        var att3 = insp.GroupBy(a => a.FieldName == "UID");
                        var att4 = insp.Any(a => a.FieldName == "UID");
                        var att5 = insp.Where(a => a.FieldName == "UID")*/
                        //insp["UID"] = "newAtr";
                        
                        
                    });

                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }

        

        private async Task<string> GetDataSource(BasicFeatureLayer theLayer)
        {
            try
            {
                return await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    var inTable = theLayer.Name;
                    var table = theLayer.GetTable();
                    var dataStore = table.GetDatastore();
                    var workspaceNameDef = dataStore.GetConnectionString();
                    var workspaceName = workspaceNameDef.Split('=')[1];

                    var fullSpec = System.IO.Path.Combine(workspaceName, inTable);
                    return fullSpec;
                });
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                return string.Empty;
            }
        }

        private async Task<bool> ExecuteAddFieldTool(BasicFeatureLayer theLayer, KeyValuePair<string, string> field, string fieldType, int? fieldLength = null, bool isNullable = true)
        {
            try
            {
                return await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    var inTable = theLayer.Name;
                    var table = theLayer.GetTable();
                    var dataStore = table.GetDatastore();
                    var workspaceNameDef = dataStore.GetConnectionString();
                    var workspaceName = workspaceNameDef.Split('=')[1];

                    var fullSpec = System.IO.Path.Combine(workspaceName, inTable);
                    System.Diagnostics.Debug.WriteLine($@"Add {field.Key} from {fullSpec}");

                    var parameters = Geoprocessing.MakeValueArray(fullSpec, field.Key, fieldType.ToUpper(), null, null,
                        fieldLength, field.Value, isNullable ? "NULABLE" : "NON_NULLABLE");
                    var cts = new CancellationTokenSource();
                    var results = Geoprocessing.ExecuteToolAsync("management.AddField", parameters, null, cts.Token,
                        (eventName, o) =>
                        {
                            System.Diagnostics.Debug.WriteLine($@"GP event: {eventName}");
                        });
                    return true;
                });
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                return false;
            }
        }
    }
}
