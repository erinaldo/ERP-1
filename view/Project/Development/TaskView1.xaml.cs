using entity;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.TreeGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Cognitivo.Project.Development
{
    public partial class TaskView1 : Page, INotifyPropertyChanged
    {


        // private ProjectTaskDB ProjectTaskDB = new ProjectTaskDB();
        private entity.Controller.Project.ProjectController ProjectDB;
        private CollectionViewSource projectViewSource;
        private cntrl.PanelAdv.Project_TaskApprove Project_TaskApprove;
        List<project_task> projectasklist = new List<project_task>();
        /// <summary>
        /// Property used by Open TreeView Button.
        /// </summary>
        public bool ViewAll { get; set; }

        public TaskView1()
        {
            InitializeComponent();

            ProjectDB = FindResource("ProjectDB") as entity.Controller.Project.ProjectController;


            ProjectDB.Initialize();

        }


        private void Load()
        {
            ProjectDB.Load(dataPager.PagedSource.PageIndex);
            projectViewSource = FindResource("projectViewSource") as CollectionViewSource;


            projectViewSource.Source = ProjectDB.db.projects.Local;


            if (dataPager.PageCount == 0)
            {
                dataPager.PageCount = ProjectDB.PageCount;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {







            CollectionViewSource app_dimensionViewSource = ((CollectionViewSource)(FindResource("app_dimensionViewSource")));
            app_dimensionViewSource.Source = ProjectDB.db.app_dimension.Local;

            CollectionViewSource app_propertyViewSource = ((CollectionViewSource)(FindResource("app_propertyViewSource")));

            app_propertyViewSource.Source = ProjectDB.db.app_property.Local;

            CollectionViewSource app_measurementViewSource = ((CollectionViewSource)(FindResource("app_measurementViewSource")));

            app_measurementViewSource.Source = ProjectDB.db.app_measurement.Local;
            cbxItemType.ItemsSource = Enum.GetValues(typeof(item.item_type));



            entity.Brillo.Security security = new entity.Brillo.Security(entity.App.Names.ActivityPlan);

            if (security.approve)
            {
                btnapprove.IsEnabled = true;
            }
            else
            {
                btnapprove.IsEnabled = false;
            }

            if (security.annul)
            {
                btnanull.IsEnabled = true;
            }
            else
            {
                btnanull.IsEnabled = false;
            }

            //if (ToggleQuantity.IsChecked == true)
            //{
            //    stpexcustion.Visibility = Visibility.Visible;
            //    stpestimate.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    stpestimate.Visibility = Visibility.Visible;
            //    stpexcustion.Visibility = Visibility.Collapsed;
            //}
        }



        #region Toolbar Events

        private void toolBar_btnSearch_Click(object sender, string query)
        {
            try
            {
                projectViewSource.View.Filter = i =>
                {
                    project project = i as project;

                    string name = project.name != null ? project.name : "";
                    string code = project.code != null ? project.code : "";

                    if (
                        name.ToLower().Contains(query.ToLower()) ||
                        project.project_tag_detail.Where(x => x.project_tag.name.ToLower().Contains(query.ToLower())).Any() ||
                        code.ToLower().Contains(query.ToLower())
                        )
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                };
            }
            catch (Exception ex)
            {
                toolBar.msgError(ex);
            }


        }

        private void toolBar_btnApprove_Click(object sender)
        {
            //if (project_taskViewSource.View != null)
            //{
            //    project_taskViewSource.View.Filter = null;
            //List<project_task> _project_task = treeProject.ItemsSource.Cast<project_task>().ToList();
            List<project_task> _project_task = dgvtaskview.SelectedItems.Cast<project_task>().ToList();

            if (_project_task != null)
            {
                _project_task = _project_task.Where(x => x.IsSelected == true).ToList();
                app_document_range app_document_range = ProjectDB.db.app_document_range.Where(x => x.id_range == Project_TaskApprove.id_range).FirstOrDefault();
                string number = entity.Brillo.Logic.Range.calc_Range(app_document_range, true);
                foreach (project_task project_task in _project_task)
                {
                    project_task.project.code = Project_TaskApprove.code;
                    if (Project_TaskApprove.id_range != null)
                    {
                        project_task.id_range = Project_TaskApprove.id_range;
                        // app_document_range = ProjectTaskDB.app_document_range.Where(x => x.id_range == Project_TaskApprove.id_range).FirstOrDefault();
                        if (app_document_range != null)
                        {
                            project_task.app_document_range = app_document_range;
                            project_task.number = number;
                        }
                    }
                    // project_task.number = number;

                    if (project_task.status == Status.Project.Management_Approved)
                    {
                        project_task.status = Status.Project.Approved;
                    }
                    project_task.IsSelected = false;
                }

                ProjectDB.SaveChanges_WithValidation();
                if (_project_task.FirstOrDefault() != null)
                {
                    if (_project_task.FirstOrDefault().app_document_range != null)
                    {
                        entity.Brillo.Document.Start.Automatic(_project_task.FirstOrDefault().project, _project_task.FirstOrDefault().app_document_range);
                    }
                }


            }
            // }
        }

        private void toolBar_btnAnull_Click(object sender)
        {
            //if (project_taskViewSource.View != null)
            //{
            //    project_taskViewSource.View.Filter = null;

            List<project_task> project_taskLIST = dgvtaskview.SelectedItems.Cast<project_task>().ToList();
            // List<project_task> project_taskLIST = treeProject.ItemsSource.Cast<project_task>().ToList();
            if (project_taskLIST != null)
            {
                project_taskLIST = project_taskLIST.Where(x => x.IsSelected == true).ToList();

                foreach (project_task project_task in project_taskLIST)
                {
                    if (project_task.items.id_item_type != item.item_type.Task)
                    {
                        project_task.status = Status.Project.Rejected;
                    }
                    project_task.IsSelected = false;
                }

                //Saving Changes
                if (ProjectDB.SaveChanges_WithValidation())
                {
                    toolBar.msgAnnulled(ProjectDB.NumberOfRecords);
                }


            }
            // }
        }

        private void toolBar_btnNew_Click(object sender)
        {
            crud_modal.Visibility = Visibility.Visible;
            cntrl.Curd.project project = new cntrl.Curd.project();
            crud_modal.Children.Add(project);
        }

        private void toolBar_btnDelete_Click(object sender)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure want to Delete?", "Cognitivo", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (res == MessageBoxResult.Yes)
            {
                project projects = projectViewSource.View.CurrentItem as project;
                projects.is_active = false;
                ProjectDB.SaveChanges_WithValidation();
                projectViewSource.View.Filter = i =>
                {
                    project objProject = (project)i;
                    if (objProject.is_active == true)
                        return true;
                    else
                        return false;
                };
            }
        }

        private void toolBar_btnEdit_Click(object sender)
        {
            crud_modal.Visibility = Visibility.Visible;
            cntrl.Curd.project project = new cntrl.Curd.project();
            project.project_crud = (project)projectViewSource.View.CurrentItem;
            crud_modal.Children.Add(project);
        }

        #endregion Toolbar Events

        #region Project Task Events

        private void btnNewTask_Click(object sender)
        {
            //stpcode.IsEnabled = true;

            //project project = projectViewSource.View.CurrentItem as project;
            //project_task project_task = treeProject.SelectedItem_ as project_task;

            //if (project != null)
            //{
            //    if (project_task != null && project_task.items != null && project_task.items.item_recepie.Count() == 0)
            //    {
            //        //Adding a Child Item.
            //        if (project_task.items != null)
            //        {
            //            if (project_task.items.id_item_type == item.item_type.Task)
            //            {
            //                project_task n_project_task = new project_task();
            //                n_project_task.id_project = project.id_project;
            //                n_project_task.status = Status.Project.Pending;
            //                n_project_task.quantity_est = 0;
            //                n_project_task.parent = project_task;
            //                n_project_task.State = EntityState.Added;
            //                project_task.child.Add(n_project_task);
            //                ProjectDB.db.project_task.Add(n_project_task);
            //                project_taskViewSource.View.Refresh();
            //                project_taskViewSource.View.MoveCurrentTo(n_project_task);

            //                treeProject.SelectedItem_ = n_project_task;

            //            }
            //        }
            //    }
            //    else
            //    {
            //        project_task n_project_task = new project_task();
            //        n_project_task.id_project = project.id_project;
            //        n_project_task.status = entity.Status.Project.Pending;
            //        n_project_task.State = EntityState.Added;
            //        ProjectDB.db.project_task.Add(n_project_task);

            //        project_taskViewSource.View.Filter = null;
            //        project_taskViewSource.View.MoveCurrentTo(n_project_task);
            //        treeProject.SelectedItem_ = n_project_task;
            //       
            //    }
            //}
        }

        private void btnAddParentTask_Click(object sender)
        {
            dgvtaskview.ItemsSource = null;

            //stpcode.IsEnabled = true;
            project project = projectViewSource.View.CurrentItem as project;

            if (project != null)
            {
                int? sequence = projectasklist.Where(x => x.parent == null).Max(x => x.sequence);
                project_task n_project_task = new project_task();
                n_project_task.id_project = project.id_project;
                n_project_task.status = entity.Status.Project.Pending;
                n_project_task.State = EntityState.Added;
                n_project_task.sequence = sequence + 1;
                ProjectDB.db.project_task.Add(n_project_task);
                projectasklist.Add(n_project_task);
                LoadTask(project);
                dgvtaskview.SelectedItem = n_project_task;
            }
            else
            {
                toolBar.msgWarning(entity.Brillo.Localize.PleaseSelect);
            }


        }

        private void btnEditTask_Click(object sender)
        {
            //stpcode.IsEnabled = true;
            //project_task project_task = treeProject.SelectedItem_ as project_task;
            //if (project_task != null)
            //{
            //    project_task.State = EntityState.Modified;
            //}
            //else
            //{
            //    toolBar.msgWarning(entity.Brillo.Localize.PleaseSelect);
            //}
        }

        private void btnSaveTask_Click(object sender)
        {
            if (ProjectDB.SaveChanges_WithValidation())
            {
                toolBar.msgSaved(ProjectDB.NumberOfRecords);
                //stpcode.IsEnabled = false;
            }
        }

        private void btnDeleteTask_Click(object sender)
        {

            List<project_task> _project_task = dgvtaskview.SelectedItems.Cast<project_task>().ToList();
            project project = projectViewSource.View.CurrentItem as project;
            ProjectDB.NumberOfRecords = 0;
            foreach (project_task task in _project_task)
            {
                if (task.status == Status.Project.Pending)
                {
                    using (db db = new db())
                    {
                        if (task.id_project_task != 0)
                        {
                            db.project_task.Remove(db.project_task.Where(x => x.id_project_task == task.id_project_task).FirstOrDefault());
                            db.SaveChanges();
                        }
                        else
                        {
                            ProjectDB.db.Entry(task).State = EntityState.Detached;
                        }
                    }

                    ProjectDB.NumberOfRecords += 1;
                }
                else
                {
                    //ProjectTaskDB.SaveChanges();
                    toolBar_btnAnull_Click(sender);
                }
            }


            ProjectDB.db.SaveChanges();
            LoadTask(project);



        }

        private void btnApprove_Click(object sender)
        {
            ProjectDB.NumberOfRecords += 0;



            List<project_task> _project_task = dgvtaskview.SelectedItems.Cast<project_task>().ToList();
            // _project_task = _project_task.Where(x => x.IsSelected == true).ToList();

            foreach (project_task project_task in _project_task)
            {
                if ((project_task.status == Status.Project.Pending || project_task.status == null) && project_task.id_item > 0)
                {
                    project_task.status = Status.Project.Management_Approved;
                    ProjectDB.NumberOfRecords += 1;
                }

                project_task.IsSelected = false;
            }

            if (ProjectDB.SaveChanges_WithValidation())
            {
                toolBar.msgApproved(ProjectDB.NumberOfRecords);
            }



        }

        private void btnAnull_Click(object sender)
        {
            ProjectDB.NumberOfRecords += 0;

            List<project_task> project_taskLIST = dgvtaskview.SelectedItems.Cast<project_task>().ToList();
            foreach (project_task project_task in project_taskLIST)
            {
                if (project_task.items.id_item_type != item.item_type.Task)
                {
                    project_task.status = Status.Project.Rejected;
                }
                ProjectDB.NumberOfRecords += 1;
                project_task.IsSelected = false;

                foreach (production_order_detail production_order_detail in project_task.production_order_detail)
                {
                    if (production_order_detail.item.id_item_type != item.item_type.Task)
                    {
                        production_order_detail.status = Status.Production.Anull;
                    }
                }
            }

            if (ProjectDB.SaveChanges_WithValidation())
            {
                toolBar.msgAnnulled(ProjectDB.NumberOfRecords);
            }



        }

        #endregion Project Task Events

        private void btnChild_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //stpcode.Visibility = Visibility.Visible;
        }

        private void cbxItemType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBox cbxItemType = (ComboBox)sender;
            //project_task project_task = (project_task)treeProject.SelectedItem_;

            if (cbxItemType.SelectedItem != null)
            {
                item.item_type Item_Type = (item.item_type)cbxItemType.SelectedItem;
                sbxItem.item_types = Item_Type;

                if (Item_Type == item.item_type.Task)
                {
                    stpdate.Visibility = Visibility.Visible;
                    stpdate.IsEnabled = true;
                    stpitem.Visibility = Visibility.Collapsed;
                    stpitem.IsEnabled = false;
                }
                else
                {
                    stpdate.Visibility = Visibility.Collapsed;
                    stpdate.IsEnabled = false;
                    stpitem.Visibility = Visibility.Visible;
                    stpitem.IsEnabled = true;
                }

                if (Item_Type == item.item_type.Service)
                {
                    stackDimension.Visibility = Visibility.Hidden;
                }
                else
                {
                    stackDimension.Visibility = Visibility.Visible;
                }
            }
        }



        private void crud_modal_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (crud_modal.Visibility == Visibility.Hidden)
            {
                projectViewSource = ((CollectionViewSource)(FindResource("projectViewSource")));
                ProjectDB.db.Database.Initialize(true);
                projectViewSource.Source = ProjectDB.db.projects.Where(a => a.is_active == true && a.id_company == entity.CurrentSession.Id_Company).ToList();
                projectViewSource.View.MoveCurrentToLast();

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            crud_modal.Visibility = Visibility.Visible;
            Project_TaskApprove = new cntrl.PanelAdv.Project_TaskApprove();
            Project_TaskApprove.Save_Click += toolBar_btnApprove_Click;
            crud_modal.Children.Add(Project_TaskApprove);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            toolBar_btnAnull_Click(sender);
        }

        private void item_Select(object sender, RoutedEventArgs e)
        {
            if (sbxItem.ItemID > 0)
            {
                item item = ProjectDB.db.items.Find(sbxItem.ItemID);
                project project = projectViewSource.View.CurrentItem as project;
                project_task project_task_output = dgvtaskview.SelectedItem as project_task;

                if (project_task_output != null)
                {
                    if (project_task_output.parent != null)
                    {
                        if (project_task_output.parent.items.is_autorecepie)
                        {
                            MessageBox.Show("Can't add becuse item is Auto-Recipe");
                        }
                    }
                    if (item != null && item.id_item > 0 && item.is_autorecepie)
                    {
                        project_task_output.id_item = item.id_item;
                        project_task_output.items = item;
                        project_task_output.RaisePropertyChanged("item");
                        project_task_output.quantity_est = 1;
                        foreach (item_recepie_detail item_recepie_detail in item.item_recepie.FirstOrDefault().item_recepie_detail)
                        {
                            project_task project_task = new project_task();

                            project_task.code = item_recepie_detail.item.name;
                            project_task.item_description = item_recepie_detail.item.name;
                            project_task.id_item = item_recepie_detail.item.id_item;
                            project_task.items = item_recepie_detail.item;
                            project_task.id_project = project_task_output.id_project;
                            project_task.status = Status.Project.Pending;
                            project_task.RaisePropertyChanged("item");
                            if (item_recepie_detail.quantity > 0)
                            {
                                project_task.quantity_est = (decimal)item_recepie_detail.quantity;
                            }

                            project_task_output.child.Add(project_task);
                        }

                    }
                    else
                    {
                        if (project_task_output.code == "")
                        {
                            project_task_output.code = item.code;
                        }
                        project_task_output.id_item = item.id_item;
                        project_task_output.items = item;
                        project_task_output.item_description = item.name;
                        project_task_output.RaisePropertyChanged("item_description");
                        project_task_output.RaisePropertyChanged("item");
                        project_task_output.quantity_est = 1;
                    }

                    if (item.item_dimension.Count() > 0)
                    {


                        ProjectDB.db.project_task_dimension.RemoveRange(project_task_output.project_task_dimension);
                        foreach (item_dimension _item_dimension in item.item_dimension)
                        {
                            project_task_dimension project_task_dimension = new project_task_dimension();
                            project_task_dimension.id_dimension = _item_dimension.id_app_dimension;
                            project_task_dimension.app_dimension = _item_dimension.app_dimension;
                            project_task_dimension.value = _item_dimension.value;
                            project_task_dimension.id_measurement = _item_dimension.id_measurement;
                            project_task_dimension.app_measurement = _item_dimension.app_measurement;

                            project_task_output.project_task_dimension.Add(project_task_dimension);
                            project_task_output.RaisePropertyChanged("project_task_dimension");
                        }
                    }




                    project_task_output.CalcSalePrice_TimerTaks();
                }




                dgvtaskview.ItemsSource = null;
                ProjectDB.db.SaveChanges();
                LoadTask(project);


            }

        }

        private void btnExpandAll_Checked(object sender, RoutedEventArgs e)
        {
            dgvtaskview.ItemsSource = null;

            project projects = projectViewSource.View.CurrentItem as project;
            LoadTask(projects);


        }
        public void LoadTask(project projects)
        {
            if (projectasklist.Count() == 0)
            {
                projectasklist = ProjectDB.db.project_task.Where(x => x.id_project == projects.id_project)
                    .Include(x => x.items)
                    .Include(x => x.project_task_dimension)
                    .ToList();
            }

            dgvtaskview.ItemsSource = projectasklist;
            cbxParentTask.ItemsSource = projectasklist;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void ToggleQuantity_Checked(object sender, RoutedEventArgs e)
        {
            if (ToggleQuantity.IsChecked == true)
            {
                //stpexcustion.Visibility = Visibility.Visible;
                stpestimate.Visibility = Visibility.Collapsed;
            }
        }

        private void ToggleQuantity_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ToggleQuantity.IsChecked == false)
            {
                stpestimate.Visibility = Visibility.Visible;
                //stpexcustion.Visibility = Visibility.Collapsed;
            }
        }

        private void cbxproperty_SelectionChanged(object sender, EventArgs e)
        {
            if (cbxproperty.SelectedItem != null)
            {
                project_task project_task_output = dgvtaskview.SelectedItem as project_task;

                if (project_task_output != null)
                {
                    project_task_output.item_description = project_task_output.item_description + " || POS: '" + cbxproperty.Text + "'";
                    project_task_output.RaisePropertyChanged("item_description");
                }
            }
        }

        private void Slider_LostFocus(object sender, RoutedEventArgs e)
        {
            project_task project_task = dgvtaskview.SelectedItem as project_task;
            if (project_task != null)
            {
                using (db db = new db())
                {
                    project_task _project_task = db.project_task.Find(project_task.id_project_task);

                    try
                    {
                        _project_task.completed = project_task.completed;
                        _project_task.importance = project_task.importance;
                        _project_task.RaisePropertyChanged("importance");
                        db.SaveChanges();
                    }
                    catch
                    {
                        //Do nothing
                    }
                }
            }
        }

        private void toolBar_btnFocus_Click(object sender)
        {
            if (toolBar.ref_id > 0)
            {
                projectViewSource = FindResource("projectViewSource") as CollectionViewSource;
                projectViewSource.Source = ProjectDB.db.projects.Where(x => x.id_project == toolBar.ref_id).ToList();
            }
        }

        private void toolBar_btnClear_Click(object sender)
        {
            ProjectDB.Initialize();
            Load();
        }
        private void dataPager_OnDemandLoading(object sender, Syncfusion.UI.Xaml.Controls.DataPager.OnDemandLoadingEventArgs e)
        {
            Load();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void download_excel_click(object sender, RoutedEventArgs e)
        {
            entity.Brillo.Task2Excel Task2Excel = new entity.Brillo.Task2Excel();
            project project = projectViewSource.View.CurrentItem as project;
            if (project != null)
            {
                Task2Excel.Create(project);
            }

        }




        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            projectasklist.Clear();
            dgvtaskview.ItemsSource = null;
        }

        private void MenuLevelUp_Click(object sender, RoutedEventArgs e)
        {

            project_task project_task = dgvtaskview.SelectedItem as project_task;
            project projects = projectViewSource.View.CurrentItem as project;
            if (project_task.parent != null && project_task.parent.parent != null)
            {
                project_task.parent_child = project_task.parent.parent.id_project_task;
                project_task.parent = project_task.parent.parent;
                int sequence = 0;
                foreach (project_task parent in projects.project_task.Where(x => x.parent == project_task.parent))
                {
                    sequence = sequence + 1;
                    parent.SetSequence(sequence);
                }

                //ProjectDB.db.SaveChanges();
                btnExpandAll_Checked(null, null);
            }


        }

        private void MenuNew_Click(object sender, RoutedEventArgs e)
        {


            project project = projectViewSource.View.CurrentItem as project;
            project_task project_task = dgvtaskview.SelectedItem as project_task;

            if (project != null)
            {
                if (project_task != null && project_task.items != null && project_task.items.item_recepie.Count() == 0)
                {
                    //Adding a Child Item.
                    if (project_task.items != null)
                    {
                        if (project_task.items.id_item_type == item.item_type.Task)
                        {
                            int? sequence = projectasklist.Where(x => x.parent == project_task).Max(x => x.sequence);
                            project_task n_project_task = new project_task();
                            n_project_task.id_project = project.id_project;
                            n_project_task.status = Status.Project.Pending;
                            n_project_task.quantity_est = 0;
                            n_project_task.parent = project_task;
                            n_project_task.State = EntityState.Added;
                            n_project_task.sequence = sequence + 1;
                            n_project_task.parent_child = project_task.id_project_task;
                            project_task.child.Add(n_project_task);
                            ProjectDB.db.project_task.Add(n_project_task);
                            projectasklist.Add(n_project_task);

                        }
                    }
                }
                else
                {
                    project_task n_project_task = new project_task();
                    n_project_task.id_project = project.id_project;
                    n_project_task.status = entity.Status.Project.Pending;
                    n_project_task.State = EntityState.Added;
                    ProjectDB.db.project_task.Add(n_project_task);
                    projectasklist.Add(n_project_task);

                }
            }



            //ProjectDB.db.SaveChanges();
            dgvtaskview.ItemsSource = null;
            LoadTask(project);

        }





        private void Close_model(object sender, RoutedEventArgs e)
        {
            parent_level_modal.Visibility = Visibility.Hidden;
        }

        private void ReAssignLevel_click(object sender, RoutedEventArgs e)
        {
            project_task project_task = dgvtaskview.SelectedItem as project_task;
            project projects = projectViewSource.View.CurrentItem as project;
            if (cbxParentTask.SelectedItem != null)
            {
                project_task parent_project_task = cbxParentTask.SelectedItem as project_task;
                project_task.parent_child = parent_project_task.id_project_task;
                project_task.parent = parent_project_task;
                // ProjectDB.db.SaveChanges();
                LoadTask(projects);
            }
            Close_model(null, null);


        }

        private void MenuReAssignLevel_Click(object sender, RoutedEventArgs e)
        {
            parent_level_modal.Visibility = Visibility.Visible;
        }

        private void MenuUP_Click(object sender, RoutedEventArgs e)
        {
            project projects = projectViewSource.View.CurrentItem as project;
            project_task selectedtask = dgvtaskview.SelectedItem as project_task;
            int? currentsequence = selectedtask.sequence;
            project_task selectedUPtask = projectasklist.Where(x => x.sequence == currentsequence - 1 && x.parent == selectedtask.parent).FirstOrDefault();
            if (selectedUPtask != null)
            {
                selectedtask.sequence = selectedUPtask.sequence;
                selectedUPtask.sequence = currentsequence;
            }

            btnExpandAll_Checked(null, null);
            //LoadTask(projects);
        }

        private void MenuDown_Click(object sender, RoutedEventArgs e)
        {
            project projects = projectViewSource.View.CurrentItem as project;
            project_task selectedtask = dgvtaskview.SelectedItem as project_task;
            int? currentsequence = selectedtask.sequence;
            project_task selectedUPtask = projectasklist.Where(x => x.sequence == currentsequence + 1 && x.parent == selectedtask.parent).FirstOrDefault();
            if (selectedUPtask != null)
            {
                selectedtask.sequence = selectedUPtask.sequence;
                selectedUPtask.sequence = currentsequence;
            }

            btnExpandAll_Checked(null, null);
        }

        private void dgvtaskview_CurrentCellBeginEdit(object sender, TreeGridCurrentCellBeginEditEventArgs e)
        {
            project_task selectedtask = dgvtaskview.SelectedItem as project_task;
            if (selectedtask.status!=Status.Project.Pending)
            {
                e.Cancel = true;
            }
        }
    }
}