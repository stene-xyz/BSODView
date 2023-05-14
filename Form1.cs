using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics.Eventing.Reader;
using System.Security.Principal;
using System.Diagnostics;

namespace BSODView
{
    public partial class Form1 : Form
    {

        Dictionary<string, string> translation = new Dictionary<string, string>();
        Dictionary<string, string> documentation = new Dictionary<string, string>();
        Crash[] crashes;
        Crash currentCrash = null;
        
        public void LoadEVTXFile(string filename)
        {
            currentCrash = null;
            List<Crash> crashList = new List<Crash>();
            EventLogQuery logQuery = new EventLogQuery(filename, PathType.FilePath, "*[System/Level=1]");
            try
            {
                EventLogReader logReader = new EventLogReader(logQuery);
                EventRecord eventDetail;
                while(true) {
                    try
                    {
                        eventDetail = logReader.ReadEvent();
                        if (eventDetail == null) break;
                        Crash crash = new Crash();
                        crash.timestamp = eventDetail.TimeCreated.ToString();
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(eventDetail.ToXml());
                        XmlNode eventData = doc.ChildNodes[0];
                        foreach (XmlNode data in eventData.ChildNodes)
                        {
                            if (data.Name.Equals("EventData"))
                            {
                                foreach (XmlNode dataItem in data.ChildNodes)
                                {
                                    Console.WriteLine(dataItem.InnerText);
                                    if (dataItem.Attributes["Name"].InnerText == "BugcheckCode")
                                        if (!dataItem.InnerText.Equals("0"))
                                            crash.crashType = "0x" + string.Format("{0:X8}", int.Parse(dataItem.InnerText));
                                    if (dataItem.Attributes["Name"].InnerText == "BugcheckParameter1") crash.parameters[0] = "0x" + string.Format("{0:X16}", Int64.Parse(dataItem.InnerText.Substring(2), System.Globalization.NumberStyles.HexNumber));
                                    if (dataItem.Attributes["Name"].InnerText == "BugcheckParameter2") crash.parameters[1] = "0x" + string.Format("{0:X16}", Int64.Parse(dataItem.InnerText.Substring(2), System.Globalization.NumberStyles.HexNumber));
                                    if (dataItem.Attributes["Name"].InnerText == "BugcheckParameter3") crash.parameters[2] = "0x" + string.Format("{0:X16}", Int64.Parse(dataItem.InnerText.Substring(2), System.Globalization.NumberStyles.HexNumber));
                                    if (dataItem.Attributes["Name"].InnerText == "BugcheckParameter4") crash.parameters[3] = "0x" + string.Format("{0:X16}", Int64.Parse(dataItem.InnerText.Substring(2), System.Globalization.NumberStyles.HexNumber));
                                }
                            }
                            if (data.Name.Equals("RenderingInfo"))
                            {
                                foreach (XmlNode dataItem in data.ChildNodes)
                                {
                                    if (dataItem.Name.Equals("Message"))
                                    {
                                        crash.message = dataItem.InnerText;
                                    }
                                }
                            }
                        }
                        if (crash.crashType.Length > 0) crashList.Add(crash);
                    }
                    catch(Exception e)
                    {
                        ErrorInfoForm info = new ErrorInfoForm();
                        info.UpdateText("Error reading EVTX File", e);
                        info.Show();
                        break;
                    }
                }
                crashes = crashList.ToArray();
                CrashSelector.Items.Clear();
                for (int i = 0; i < crashes.Length; i++)
                {
                    CrashSelector.Items.Add(crashes[i].timestamp);
                }
            }
            catch(Exception e)
            {
                ErrorInfoForm info = new ErrorInfoForm();
                info.UpdateText("Error reading EVTX File", e);
                info.Show();
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            LoadEVTXFile(openFileDialog1.FileName);
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            new Help().Show();
        }

        private void CrashSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            CrashInfo.Items.Clear();
            int i = CrashSelector.SelectedIndex;
            currentCrash = crashes[i];
            CrashInfo.Items.Add("Crash at " + currentCrash.timestamp + ":");
            CrashInfo.Items.Add(translation[currentCrash.crashType] + " (bugcheck code " + currentCrash.crashType + ")");
            CrashInfo.Items.Add(currentCrash.message);
            CrashInfo.Items.Add("Parameter 1: " + crashes[i].parameters[0]);
            CrashInfo.Items.Add("Parameter 2: " + crashes[i].parameters[1]);
            CrashInfo.Items.Add("Parameter 3: " + crashes[i].parameters[2]);
            CrashInfo.Items.Add("Parameter 4: " + crashes[i].parameters[3]);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Check for admin privs
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if(!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                var exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                startInfo.Verb = "runas";
                System.Diagnostics.Process.Start(startInfo);
                Application.Exit();
            }

            // Load the current system's logs
            LoadEVTXFile("C:\\Windows\\System32\\winevt\\Logs\\System.evtx");
        }

        private void loadFromSystemButton_Click(object sender, EventArgs e)
        {
            LoadEVTXFile("C:\\Windows\\System32\\winevt\\Logs\\System.evtx");
        }

        private void loadFromDriveButton_Click(object sender, EventArgs e)
        {
            new DriveSelector().Show();
        }

        public Form1()
        {
            InitializeComponent();
            translation.Add("0x00000000", "POWER_LOSS_OR_HARDWARE_FAILURE");
            translation.Add("0x00000001", "APC_INDEX_MISMATCH");
            translation.Add("0x00000002", "DEVICE_QUEUE_NOT_BUSY");
            translation.Add("0x00000003", "INVALID_AFFINITY_SET");
            translation.Add("0x00000004", "INVALID_DATA_ACCESS_TRAP");
            translation.Add("0x00000005", "INVALID_PROCESS_ATTACH_ATTEMPT");
            translation.Add("0x00000006", "INVALID_PROCESS_DETACH_ATTEMPT");
            translation.Add("0x00000007", "INVALID_SOFTWARE_INTERRUPT");
            translation.Add("0x00000008", "IRQL_NOT_DISPATCH_LEVEL");
            translation.Add("0x00000009", "IRQL_NOT_GREATER_OR_EQUAL");
            translation.Add("0x0000000A", "IRQL_NOT_LESS_OR_EQUAL");
            translation.Add("0x0000000B", "NO_EXCEPTION_HANDLING_SUPPORT");
            translation.Add("0x0000000C", "MAXIMUM_WAIT_OBJECTS_EXCEEDED");
            translation.Add("0x0000000D", "MUTEX_LEVEL_NUMBER_VIOLATION");
            translation.Add("0x0000000E", "NO_USER_MODE_CONTEXT");
            translation.Add("0x0000000F", "SPIN_LOCK_ALREADY_OWNED");
            translation.Add("0x00000010", "SPIN_LOCK_NOT_OWNED");
            translation.Add("0x00000011", "THREAD_NOT_MUTEX_OWNER");
            translation.Add("0x00000012", "TRAP_CAUSE_UNKNOWN");
            translation.Add("0x00000013", "EMPTY_THREAD_REAPER_LIST");
            translation.Add("0x00000014", "CREATE_DELETE_LOCK_NOT_LOCKED");
            translation.Add("0x00000015", "LAST_CHANCE_CALLED_FROM_KMODE");
            translation.Add("0x00000016", "CID_HANDLE_CREATION");
            translation.Add("0x00000017", "CID_HANDLE_DELETION");
            translation.Add("0x00000018", "REFERENCE_BY_POINTER");
            translation.Add("0x00000019", "BAD_POOL_HEADER");
            translation.Add("0x0000001A", "MEMORY_MANAGEMENT");
            translation.Add("0x0000001B", "PFN_SHARE_COUNT");
            translation.Add("0x0000001C", "PFN_REFERENCE_COUNT");
            translation.Add("0x0000001D", "NO_SPIN_LOCK_AVAILABLE");
            translation.Add("0x0000001E", "KMODE_EXCEPTION_NOT_HANDLED");
            translation.Add("0x0000001F", "SHARED_RESOURCE_CONV_ERROR");
            translation.Add("0x00000020", "KERNEL_APC_PENDING_DURING_EXIT");
            translation.Add("0x00000021", "QUOTA_UNDERFLOW");
            translation.Add("0x00000022", "FILE_SYSTEM");
            translation.Add("0x00000023", "FAT_FILE_SYSTEM");
            translation.Add("0x00000024", "NTFS_FILE_SYSTEM");
            translation.Add("0x00000025", "NPFS_FILE_SYSTEM");
            translation.Add("0x00000026", "CDFS_FILE_SYSTEM");
            translation.Add("0x00000027", "RDR_FILE_SYSTEM");
            translation.Add("0x00000028", "CORRUPT_ACCESS_TOKEN");
            translation.Add("0x00000029", "SECURITY_SYSTEM");
            translation.Add("0x0000002A", "INCONSISTENT_IRP");
            translation.Add("0x0000002B", "PANIC_STACK_SWITCH");
            translation.Add("0x0000002C", "PORT_DRIVER_INTERNAL");
            translation.Add("0x0000002D", "SCSI_DISK_DRIVER_INTERNAL");
            translation.Add("0x0000002E", "DATA_BUS_ERROR");
            translation.Add("0x0000002F", "INSTRUCTION_BUS_ERROR");
            translation.Add("0x00000030", "SET_OF_INVALID_CONTEXT");
            translation.Add("0x00000031", "PHASE0_INITIALIZATION_FAILED");
            translation.Add("0x00000032", "PHASE1_INITIALIZATION_FAILED");
            translation.Add("0x00000033", "UNEXPECTED_INITIALIZATION_CALL");
            translation.Add("0x00000034", "CACHE_MANAGER");
            translation.Add("0x00000035", "NO_MORE_IRP_STACK_LOCATIONS");
            translation.Add("0x00000036", "DEVICE_REFERENCE_COUNT_NOT_ZERO");
            translation.Add("0x00000037", "FLOPPY_INTERNAL_ERROR");
            translation.Add("0x00000038", "SERIAL_DRIVER_INTERNAL");
            translation.Add("0x00000039", "SYSTEM_EXIT_OWNED_MUTEX");
            translation.Add("0x0000003A", "SYSTEM_UNWIND_PREVIOUS_USER");
            translation.Add("0x0000003B", "SYSTEM_SERVICE_EXCEPTION");
            translation.Add("0x0000003C", "INTERRUPT_UNWIND_ATTEMPTED");
            translation.Add("0x0000003D", "INTERRUPT_EXCEPTION_NOT_HANDLED");
            translation.Add("0x0000003E", "MULTIPROCESSOR_CONFIGURATION_NOT_SUPPORTED");
            translation.Add("0x0000003F", "NO_MORE_SYSTEM_PTES");
            translation.Add("0x00000040", "TARGET_MDL_TOO_SMALL");
            translation.Add("0x00000041", "MUST_SUCCEED_POOL_EMPTY");
            translation.Add("0x00000042", "ATDISK_DRIVER_INTERNAL");
            translation.Add("0x00000043", "NO_SUCH_PARTITION");
            translation.Add("0x00000044", "MULTIPLE_IRP_COMPLETE_REQUESTS");
            translation.Add("0x00000045", "INSUFFICIENT_SYSTEM_MAP_REGS");
            translation.Add("0x00000046", "DEREF_UNKNOWN_LOGON_SESSION");
            translation.Add("0x00000047", "REF_UNKNOWN_LOGON_SESSION");
            translation.Add("0x00000048", "CANCEL_STATE_IN_COMPLETED_IRP");
            translation.Add("0x00000049", "PAGE_FAULT_WITH_INTERRUPTS_OFF");
            translation.Add("0x0000004A", "IRQL_GT_ZERO_AT_SYSTEM_SERVICE");
            translation.Add("0x0000004B", "STREAMS_INTERNAL_ERROR");
            translation.Add("0x0000004C", "FATAL_UNHANDLED_HARD_ERROR");
            translation.Add("0x0000004D", "NO_PAGES_AVAILABLE");
            translation.Add("0x0000004E", "PFN_LIST_CORRUPT");
            translation.Add("0x0000004F", "NDIS_INTERNAL_ERROR");
            translation.Add("0x00000050", "PAGE_FAULT_IN_NONPAGED_AREA");
            translation.Add("0x00000051", "REGISTRY_ERROR");
            translation.Add("0x00000052", "MAILSLOT_FILE_SYSTEM");
            translation.Add("0x00000053", "NO_BOOT_DEVICE");
            translation.Add("0x00000054", "LM_SERVER_INTERNAL_ERROR");
            translation.Add("0x00000055", "DATA_COHERENCY_EXCEPTION");
            translation.Add("0x00000056", "INSTRUCTION_COHERENCY_EXCEPTION");
            translation.Add("0x00000057", "XNS_INTERNAL_ERROR");
            translation.Add("0x00000058", "FTDISK_INTERNAL_ERROR");
            translation.Add("0x00000059", "PINBALL_FILE_SYSTEM");
            translation.Add("0x0000005A", "CRITICAL_SERVICE_FAILED");
            translation.Add("0x0000005B", "SET_ENV_VAR_FAILED");
            translation.Add("0x0000005C", "HAL_INITIALIZATION_FAILED");
            translation.Add("0x0000005D", "UNSUPPORTED_PROCESSOR");
            translation.Add("0x0000005E", "OBJECT_INITIALIZATION_FAILED");
            translation.Add("0x0000005F", "SECURITY_INITIALIZATION_FAILED");
            translation.Add("0x00000060", "PROCESS_INITIALIZATION_FAILED");
            translation.Add("0x00000061", "HAL1_INITIALIZATION_FAILED");
            translation.Add("0x00000062", "OBJECT1_INITIALIZATION_FAILED");
            translation.Add("0x00000063", "SECURITY1_INITIALIZATION_FAILED");
            translation.Add("0x00000064", "SYMBOLIC_INITIALIZATION_FAILED");
            translation.Add("0x00000065", "MEMORY1_INITIALIZATION_FAILED");
            translation.Add("0x00000066", "CACHE_INITIALIZATION_FAILED");
            translation.Add("0x00000067", "CONFIG_INITIALIZATION_FAILED");
            translation.Add("0x00000068", "FILE_INITIALIZATION_FAILED");
            translation.Add("0x00000069", "IO1_INITIALIZATION_FAILED");
            translation.Add("0x0000006A", "LPC_INITIALIZATION_FAILED");
            translation.Add("0x0000006B", "PROCESS1_INITIALIZATION_FAILED");
            translation.Add("0x0000006C", "REFMON_INITIALIZATION_FAILED");
            translation.Add("0x0000006D", "SESSION1_INITIALIZATION_FAILED");
            translation.Add("0x0000006E", "SESSION2_INITIALIZATION_FAILED");
            translation.Add("0x0000006F", "SESSION3_INITIALIZATION_FAILED");
            translation.Add("0x00000070", "SESSION4_INITIALIZATION_FAILED");
            translation.Add("0x00000071", "SESSION5_INITIALIZATION_FAILED");
            translation.Add("0x00000072", "ASSIGN_DRIVE_LETTERS_FAILED");
            translation.Add("0x00000073", "CONFIG_LIST_FAILED");
            translation.Add("0x00000074", "BAD_SYSTEM_CONFIG_INFO");
            translation.Add("0x00000075", "CANNOT_WRITE_CONFIGURATION");
            translation.Add("0x00000076", "PROCESS_HAS_LOCKED_PAGES");
            translation.Add("0x00000077", "KERNEL_STACK_INPAGE_ERROR");
            translation.Add("0x00000078", "PHASE0_EXCEPTION");
            translation.Add("0x00000079", "MISMATCHED_HAL");
            translation.Add("0x0000007A", "KERNEL_DATA_INPAGE_ERROR");
            translation.Add("0x0000007B", "INACCESSIBLE_BOOT_DEVICE");
            translation.Add("0x0000007C", "BUGCODE_NDIS_DRIVER");
            translation.Add("0x0000007D", "INSTALL_MORE_MEMORY");
            translation.Add("0x0000007E", "SYSTEM_THREAD_EXCEPTION_NOT_HANDLED");
            translation.Add("0x0000007F", "UNEXPECTED_KERNEL_MODE_TRAP");
            translation.Add("0x00000080", "NMI_HARDWARE_FAILURE");
            translation.Add("0x00000081", "SPIN_LOCK_INIT_FAILURE");
            translation.Add("0x00000082", "DFS_FILE_SYSTEM");
            translation.Add("0x00000085", "SETUP_FAILURE");
            translation.Add("0x0000008B", "MBR_CHECKSUM_MISMATCH");
            translation.Add("0x0000008E", "KERNEL_MODE_EXCEPTION_NOT_HANDLED");
            translation.Add("0x0000008F", "PP0_INITIALIZATION_FAILED");
            translation.Add("0x00000090", "PP1_INITIALIZATION_FAILED");
            translation.Add("0x00000092", "UP_DRIVER_ON_MP_SYSTEM");
            translation.Add("0x00000093", "INVALID_KERNEL_HANDLE");
            translation.Add("0x00000094", "KERNEL_STACK_LOCKED_AT_EXIT");
            translation.Add("0x00000096", "INVALID_WORK_QUEUE_ITEM");
            translation.Add("0x00000097", "BOUND_IMAGE_UNSUPPORTED");
            translation.Add("0x00000098", "END_OF_NT_EVALUATION_PERIOD");
            translation.Add("0x00000099", "INVALID_REGION_OR_SEGMENT");
            translation.Add("0x0000009A", "SYSTEM_LICENSE_VIOLATION");
            translation.Add("0x0000009B", "UDFS_FILE_SYSTEM");
            translation.Add("0x0000009C", "MACHINE_CHECK_EXCEPTION");
            translation.Add("0x0000009E", "USER_MODE_HEALTH_MONITOR");
            translation.Add("0x0000009F", "DRIVER_POWER_STATE_FAILURE");
            translation.Add("0x000000A0", "INTERNAL_POWER_ERROR");
            translation.Add("0x000000A1", "PCI_BUS_DRIVER_INTERNAL");
            translation.Add("0x000000A2", "MEMORY_IMAGE_CORRUPT");
            translation.Add("0x000000A3", "ACPI_DRIVER_INTERNAL");
            translation.Add("0x000000A4", "CNSS_FILE_SYSTEM_FILTER");
            translation.Add("0x000000A5", "ACPI_BIOS_ERROR");
            translation.Add("0x000000A7", "BAD_EXHANDLE");
            translation.Add("0x000000AC", "HAL_MEMORY_ALLOCATION");
            translation.Add("0x000000AD", "VIDEO_DRIVER_DEBUG_REPORT_REQUEST");
            translation.Add("0x000000B1", "BGI_DETECTED_VIOLATION");
            translation.Add("0x000000B4", "VIDEO_DRIVER_INIT_FAILURE");
            translation.Add("0x000000B8", "ATTEMPTED_SWITCH_FROM_DPC");
            translation.Add("0x000000B9", "CHIPSET_DETECTED_ERROR");
            translation.Add("0x000000BA", "SESSION_HAS_VALID_VIEWS_ON_EXIT");
            translation.Add("0x000000BB", "NETWORK_BOOT_INITIALIZATION_FAILED");
            translation.Add("0x000000BC", "NETWORK_BOOT_DUPLICATE_ADDRESS");
            translation.Add("0x000000BD", "INVALID_HIBERNATED_STATE");
            translation.Add("0x000000BE", "ATTEMPTED_WRITE_TO_READONLY_MEMORY");
            translation.Add("0x000000BF", "MUTEX_ALREADY_OWNED");
            translation.Add("0x000000C1", "SPECIAL_POOL_DETECTED_MEMORY_CORRUPTION");
            translation.Add("0x000000C2", "BAD_POOL_CALLER");
            translation.Add("0x000000C4", "DRIVER_VERIFIER_DETECTED_VIOLATION");
            translation.Add("0x000000C5", "DRIVER_CORRUPTED_EXPOOL");
            translation.Add("0x000000C6", "DRIVER_CAUGHT_MODIFYING_FREED_POOL");
            translation.Add("0x000000C7", "TIMER_OR_DPC_INVALID");
            translation.Add("0x000000C8", "IRQL_UNEXPECTED_VALUE");
            translation.Add("0x000000C9", "DRIVER_VERIFIER_IOMANAGER_VIOLATION");
            translation.Add("0x000000CA", "PNP_DETECTED_FATAL_ERROR");
            translation.Add("0x000000CB", "DRIVER_LEFT_LOCKED_PAGES_IN_PROCESS");
            translation.Add("0x000000CC", "PAGE_FAULT_IN_FREED_SPECIAL_POOL");
            translation.Add("0x000000CD", "PAGE_FAULT_BEYOND_END_OF_ALLOCATION");
            translation.Add("0x000000CE", "DRIVER_UNLOADED_WITHOUT_CANCELLING_PENDING_OPERATIONS");
            translation.Add("0x000000CF", "TERMINAL_SERVER_DRIVER_MADE_INCORRECT_MEMORY_REFERENCE");
            translation.Add("0x000000D0", "DRIVER_CORRUPTED_MMPOOL");
            translation.Add("0x000000D1", "DRIVER_IRQL_NOT_LESS_OR_EQUAL");
            translation.Add("0x000000D2", "BUGCODE_ID_DRIVER");
            translation.Add("0x000000D3", "DRIVER_PORTION_MUST_BE_NONPAGED");
            translation.Add("0x000000D4", "SYSTEM_SCAN_AT_RAISED_IRQL_CAUGHT_IMPROPER_DRIVER_UNLOAD");
            translation.Add("0x000000D5", "DRIVER_PAGE_FAULT_IN_FREED_SPECIAL_POOL");
            translation.Add("0x000000D6", "DRIVER_PAGE_FAULT_BEYOND_END_OF_ALLOCATION");
            translation.Add("0x000000D7", "DRIVER_UNMAPPING_INVALID_VIEW");
            translation.Add("0x000000D8", "DRIVER_USED_EXCESSIVE_PTES");
            translation.Add("0x000000D9", "LOCKED_PAGES_TRACKER_CORRUPTION");
            translation.Add("0x000000DA", "SYSTEM_PTE_MISUSE");
            translation.Add("0x000000DB", "DRIVER_CORRUPTED_SYSPTES");
            translation.Add("0x000000DC", "DRIVER_INVALID_STACK_ACCESS");
            translation.Add("0x000000DE", "POOL_CORRUPTION_IN_FILE_AREA");
            translation.Add("0x000000DF", "IMPERSONATING_WORKER_THREAD");
            translation.Add("0x000000E0", "ACPI_BIOS_FATAL_ERROR");
            translation.Add("0x000000E1", "WORKER_THREAD_RETURNED_AT_BAD_IRQL");
            translation.Add("0x000000E2", "MANUALLY_INITIATED_CRASH");
            translation.Add("0x000000E3", "RESOURCE_NOT_OWNED");
            translation.Add("0x000000E4", "WORKER_INVALID");
            translation.Add("0x000000E6", "DRIVER_VERIFIER_DMA_VIOLATION");
            translation.Add("0x000000E7", "INVALID_FLOATING_POINT_STATE");
            translation.Add("0x000000E8", "INVALID_CANCEL_OF_FILE_OPEN");
            translation.Add("0x000000E9", "ACTIVE_EX_WORKER_THREAD_TERMINATION");
            translation.Add("0x000000EA", "THREAD_STUCK_IN_DEVICE_DRIVER");
            translation.Add("0x000000EB", "DIRTY_MAPPED_PAGES_CONGESTION");
            translation.Add("0x000000EC", "SESSION_HAS_VALID_SPECIAL_POOL_ON_EXIT");
            translation.Add("0x000000ED", "UNMOUNTABLE_BOOT_VOLUME");
            translation.Add("0x000000EF", "CRITICAL_PROCESS_DIED");
            translation.Add("0x000000F0", "STORAGE_MINIPORT_ERROR");
            translation.Add("0x000000F1", "SCSI_VERIFIER_DETECTED_VIOLATION");
            translation.Add("0x000000F2", "HARDWARE_INTERRUPT_STORM");
            translation.Add("0x000000F3", "DISORDERLY_SHUTDOWN");
            translation.Add("0x000000F4", "CRITICAL_OBJECT_TERMINATION");
            translation.Add("0x000000F5", "FLTMGR_FILE_SYSTEM");
            translation.Add("0x000000F6", "PCI_VERIFIER_DETECTED_VIOLATION");
            translation.Add("0x000000F7", "DRIVER_OVERRAN_STACK_BUFFER");
            translation.Add("0x000000F8", "RAMDISK_BOOT_INITIALIZATION_FAILED");
            translation.Add("0x000000F9", "DRIVER_RETURNED_STATUS_REPARSE_FOR_VOLUME_OPEN");
            translation.Add("0x000000FA", "HTTP_DRIVER_CORRUPTED");
            translation.Add("0x000000FC", "ATTEMPTED_EXECUTE_OF_NOEXECUTE_MEMORY");
            translation.Add("0x000000FD", "DIRTY_NOWRITE_PAGES_CONGESTION");
            translation.Add("0x000000FE", "BUGCODE_USB_DRIVER");
            translation.Add("0x000000FF", "RESERVE_QUEUE_OVERFLOW");
            translation.Add("0x00000100", "LOADER_BLOCK_MISMATCH");
            translation.Add("0x00000101", "CLOCK_WATCHDOG_TIMEOUT");
            translation.Add("0x00000102", "DPC_WATCHDOG_TIMEOUT");
            translation.Add("0x00000103", "MUP_FILE_SYSTEM");
            translation.Add("0x00000104", "AGP_INVALID_ACCESS");
            translation.Add("0x00000105", "AGP_GART_CORRUPTION");
            translation.Add("0x00000106", "AGP_ILLEGALLY_REPROGRAMMED");
            translation.Add("0x00000108", "THIRD_PARTY_FILE_SYSTEM_FAILURE");
            translation.Add("0x00000109", "CRITICAL_STRUCTURE_CORRUPTION");
            translation.Add("0x0000010A", "APP_TAGGING_INITIALIZATION_FAILED");
            translation.Add("0x0000010C", "FSRTL_EXTRA_CREATE_PARAMETER_VIOLATION");
            translation.Add("0x0000010D", "WDF_VIOLATION");
            translation.Add("0x0000010E", "VIDEO_MEMORY_MANAGEMENT_INTERNAL");
            translation.Add("0x0000010F", "RESOURCE_MANAGER_EXCEPTION_NOT_HANDLED");
            translation.Add("0x00000111", "RECURSIVE_NMI");
            translation.Add("0x00000112", "MSRPC_STATE_VIOLATION");
            translation.Add("0x00000113", "VIDEO_DXGKRNL_FATAL_ERROR");
            translation.Add("0x00000114", "VIDEO_SHADOW_DRIVER_FATAL_ERROR");
            translation.Add("0x00000115", "AGP_INTERNAL");
            translation.Add("0x00000116", "VIDEO_TDR_FAILURE");
            translation.Add("0x00000117", "VIDEO_TDR_TIMEOUT_DETECTED");
            translation.Add("0x00000119", "VIDEO_SCHEDULER_INTERNAL_ERROR");
            translation.Add("0x0000011A", "EM_INITIALIZATION_FAILURE");
            translation.Add("0x0000011B", "DRIVER_RETURNED_HOLDING_CANCEL_LOCK");
            translation.Add("0x0000011C", "ATTEMPTED_WRITE_TO_CM_PROTECTED_STORAGE");
            translation.Add("0x0000011D", "EVENT_TRACING_FATAL_ERROR");
            translation.Add("0x0000011E", "TOO_MANY_RECURSIVE_FAULTS");
            translation.Add("0x0000011F", "INVALID_DRIVER_HANDLE");
            translation.Add("0x00000120", "BITLOCKER_FATAL_ERROR");
            translation.Add("0x00000121", "DRIVER_VIOLATION");
            translation.Add("0x00000122", "WHEA_INTERNAL_ERROR");
            translation.Add("0x00000123", "CRYPTO_SELF_TEST_FAILURE");
            translation.Add("0x00000124", "WHEA_UNCORRECTABLE_ERROR");
            translation.Add("0x00000125", "NMR_INVALID_STATE");
            translation.Add("0x00000126", "NETIO_INVALID_POOL_CALLER");
            translation.Add("0x00000127", "PAGE_NOT_ZERO");
            translation.Add("0x00000128", "WORKER_THREAD_RETURNED_WITH_BAD_IO_PRIORITY");
            translation.Add("0x00000129", "WORKER_THREAD_RETURNED_WITH_BAD_PAGING_IO_PRIORITY");
            translation.Add("0x0000012A", "MUI_NO_VALID_SYSTEM_LANGUAGE");
            translation.Add("0x0000012B", "FAULTY_HARDWARE_CORRUPTED_PAGE");
            translation.Add("0x0000012C", "EXFAT_FILE_SYSTEM");
            translation.Add("0x0000012D", "VOLSNAP_OVERLAPPED_TABLE_ACCESS");
            translation.Add("0x0000012E", "INVALID_MDL_RANGE");
            translation.Add("0x0000012F", "VHD_BOOT_INITIALIZATION_FAILED");
            translation.Add("0x00000130", "DYNAMIC_ADD_PROCESSOR_MISMATCH");
            translation.Add("0x00000131", "INVALID_EXTENDED_PROCESSOR_STATE");
            translation.Add("0x00000132", "RESOURCE_OWNER_POINTER_INVALID");
            translation.Add("0x00000133", "DPC_WATCHDOG_VIOLATION");
            translation.Add("0x00000134", "DRIVE_EXTENDER");
            translation.Add("0x00000135", "REGISTRY_FILTER_DRIVER_EXCEPTION");
            translation.Add("0x00000136", "VHD_BOOT_HOST_VOLUME_NOT_ENOUGH_SPACE");
            translation.Add("0x00000137", "WIN32K_HANDLE_MANAGER");
            translation.Add("0x00000138", "GPIO_CONTROLLER_DRIVER_ERROR");
            translation.Add("0x00000139", "KERNEL_SECURITY_CHECK_FAILURE");
            translation.Add("0x0000013A", "KERNEL_MODE_HEAP_CORRUPTION");
            translation.Add("0x0000013B", "PASSIVE_INTERRUPT_ERROR");
            translation.Add("0x0000013C", "INVALID_IO_BOOST_STATE");
            translation.Add("0x0000013D", "CRITICAL_INITIALIZATION_FAILURE");
            translation.Add("0x00000140", "STORAGE_DEVICE_ABNORMALITY_DETECTED");
            translation.Add("0x00000143", "PROCESSOR_DRIVER_INTERNAL");
            translation.Add("0x00000144", "BUGCODE_USB3_DRIVER");
            translation.Add("0x00000145", "SECURE_BOOT_VIOLATION");
            translation.Add("0x00000147", "ABNORMAL_RESET_DETECTED");
            translation.Add("0x00000149", "REFS_FILE_SYSTEM");
            translation.Add("0x0000014A", "KERNEL_WMI_INTERNAL");
            translation.Add("0x0000014B", "SOC_SUBSYSTEM_FAILURE");
            translation.Add("0x0000014C", "FATAL_ABNORMAL_RESET_ERROR");
            translation.Add("0x0000014D", "EXCEPTION_SCOPE_INVALID");
            translation.Add("0x0000014E", "SOC_CRITICAL_DEVICE_REMOVED");
            translation.Add("0x0000014F", "PDC_WATCHDOG_TIMEOUT");
            translation.Add("0x00000150", "TCPIP_AOAC_NIC_ACTIVE_REFERENCE_LEAK");
            translation.Add("0x00000151", "UNSUPPORTED_INSTRUCTION_MODE");
            translation.Add("0x00000152", "INVALID_PUSH_LOCK_FLAGS");
            translation.Add("0x00000153", "KERNEL_LOCK_ENTRY_LEAKED_ON_THREAD_TERMINATION");
            translation.Add("0x00000154", "UNEXPECTED_STORE_EXCEPTION");
            translation.Add("0x00000155", "OS_DATA_TAMPERING");
            translation.Add("0x00000157", "KERNEL_THREAD_PRIORITY_FLOOR_VIOLATION");
            translation.Add("0x00000158", "ILLEGAL_IOMMU_PAGE_FAULT");
            translation.Add("0x00000159", "HAL_ILLEGAL_IOMMU_PAGE_FAULT");
            translation.Add("0x0000015A", "SDBUS_INTERNAL_ERROR");
            translation.Add("0x0000015B", "WORKER_THREAD_RETURNED_WITH_SYSTEM_PAGE_PRIORITY_ACTIVE");
            translation.Add("0x00000160", "WIN32K_ATOMIC_CHECK_FAILURE");
            translation.Add("0x00000162", "KERNEL_AUTO_BOOST_INVALID_LOCK_RELEASE");
            translation.Add("0x00000163", "WORKER_THREAD_TEST_CONDITION");
            translation.Add("0x00000164", "WIN32K_CRITICAL_FAILURE");
            translation.Add("0x0000016C", "INVALID_RUNDOWN_PROTECTION_FLAGS");
            translation.Add("0x0000016D", "INVALID_SLOT_ALLOCATOR_FLAGS");
            translation.Add("0x0000016E", "ERESOURCE_INVALID_RELEASE");
            translation.Add("0x00000170", "CLUSTER_CSV_CLUSSVC_DISCONNECT_WATCHDOG");
            translation.Add("0x00000171", "CRYPTO_LIBRARY_INTERNAL_ERROR");
            translation.Add("0x00000173", "COREMSGCALL_INTERNAL_ERROR");
            translation.Add("0x00000174", "COREMSG_INTERNAL_ERROR");
            translation.Add("0x00000178", "ELAM_DRIVER_DETECTED_FATAL_ERROR");
            translation.Add("0x0000017B", "PROFILER_CONFIGURATION_ILLEGAL");
            translation.Add("0x0000017E", "MICROCODE_REVISION_MISMATCH");
            translation.Add("0x00000187", "VIDEO_DWMINIT_TIMEOUT_FALLBACK_BDD");
            translation.Add("0x00000189", "BAD_OBJECT_HEADER");
            translation.Add("0x0000018B", "SECURE_KERNEL_ERROR");
            translation.Add("0x0000018C", "HYPERGUARD_VIOLATION");
            translation.Add("0x0000018D", "SECURE_FAULT_UNHANDLED");
            translation.Add("0x0000018E", "KERNEL_PARTITION_REFERENCE_VIOLATION");
            translation.Add("0x00000191", "PF_DETECTED_CORRUPTION");
            translation.Add("0x00000192", "KERNEL_AUTO_BOOST_LOCK_ACQUISITION_WITH_RAISED_IRQL");
            translation.Add("0x00000196", "LOADER_ROLLBACK_DETECTED");
            translation.Add("0x00000197", "WIN32K_SECURITY_FAILURE");
            translation.Add("0x00000199", "KERNEL_STORAGE_SLOT_IN_USE");
            translation.Add("0x0000019A", "WORKER_THREAD_RETURNED_WHILE_ATTACHED_TO_SILO");
            translation.Add("0x0000019B", "TTM_FATAL_ERROR");
            translation.Add("0x0000019C", "WIN32K_POWER_WATCHDOG_TIMEOUT");
            translation.Add("0x000001A0", "TTM_WATCHDOG_TIMEOUT");
            translation.Add("0x000001A2", "WIN32K_CALLOUT_WATCHDOG_BUGCHECK");
            translation.Add("0x000001AA", "EXCEPTION_ON_INVALID_STACK");
            translation.Add("0x000001AB", "UNWIND_ON_INVALID_STACK");
            translation.Add("0x000001C6", "FAST_ERESOURCE_PRECONDITION_VIOLATION");
            translation.Add("0x000001C7", "STORE_DATA_STRUCTURE_CORRUPTION");
            translation.Add("0x000001C8", "MANUALLY_INITIATED_POWER_BUTTON_HOLD");
            translation.Add("0x000001CA", "SYNTHETIC_WATCHDOG_TIMEOUT");
            translation.Add("0x000001CB", "INVALID_SILO_DETACH");
            translation.Add("0x000001CD", "INVALID_CALLBACK_STACK_ADDRESS");
            translation.Add("0x000001CE", "INVALID_KERNEL_STACK_ADDRESS");
            translation.Add("0x000001CF", "HARDWARE_WATCHDOG_TIMEOUT");
            translation.Add("0x000001D0", "CPI_FIRMWARE_WATCHDOG_TIMEOUT");
            translation.Add("0x000001D2", "WORKER_THREAD_INVALID_STATE");
            translation.Add("0x000001D3", "WFP_INVALID_OPERATION");
            translation.Add("0x000001D5", "DRIVER_PNP_WATCHDOG");
            translation.Add("0x000001D6", "WORKER_THREAD_RETURNED_WITH_NON_DEFAULT_WORKLOAD_CLASS");
            translation.Add("0x000001D7", "EFS_FATAL_ERROR");
            translation.Add("0x000001D8", "UCMUCSI_FAILURE");
            translation.Add("0x000001D9", "HAL_IOMMU_INTERNAL_ERROR");
            translation.Add("0x000001DA", "HAL_BLOCKED_PROCESSOR_INTERNAL_ERROR");
            translation.Add("0x000001DB", "IPI_WATCHDOG_TIMEOUT");
            translation.Add("0x000001DC", "DMA_COMMON_BUFFER_VECTOR_ERROR");
            translation.Add("0x000001DD", "BUGCODE_MBBADAPTER_DRIVER");
            translation.Add("0x000001DE", "BUGCODE_WIFIADAPTER_DRIVER");
            translation.Add("0x000001E4", "VIDEO_DXGKRNL_SYSMM_FATAL_ERROR");
            translation.Add("0x000001E9", "ILLEGAL_ATS_INITIALIZATION");
            translation.Add("0x000001EA", "SECURE_PCI_CONFIG_SPACE_ACCESS_VIOLATION");
            translation.Add("0x000001EB", "DAM_WATCHDOG_TIMEOUT");
            translation.Add("0x000001ED", "HANDLE_ERROR_ON_CRITICAL_THREAD");
            translation.Add("0x00000356", "XBOX_ERACTRL_CS_TIMEOUT");
            translation.Add("0x00000BFE", "BC_BLUETOOTH_VERIFIER_FAULT");
            translation.Add("0x00000BFF", "BC_BTHMINI_VERIFIER_FAULT");
            translation.Add("0x00020001", "HYPERVISOR_ERROR");
            translation.Add("0x1000007E", "SYSTEM_THREAD_EXCEPTION_NOT_HANDLED_M");
            translation.Add("0x1000007F", "UNEXPECTED_KERNEL_MODE_TRAP_M");
            translation.Add("0x1000008E", "KERNEL_MODE_EXCEPTION_NOT_HANDLED_M");
            translation.Add("0x100000EA", "THREAD_STUCK_IN_DEVICE_DRIVER_M");
            translation.Add("0x4000008A", "THREAD_TERMINATE_HELD_MUTEX");
            translation.Add("0xC0000218", "STATUS_CANNOT_LOAD_REGISTRY_FILE");
            translation.Add("0xC000021A", "WINLOGON_FATAL_ERROR");
            translation.Add("0xC0000221", "STATUS_IMAGE_CHECKSUM_MISMATCH");
            translation.Add("0xDEADDEAD", "MANUALLY_INITIATED_CRASH1");
            documentation.Add("0x00000001", "bug-check-0x1--apc-index-mismatch");
            documentation.Add("0x00000002", "bug-check-0x2--device-queue-not-busy");
            documentation.Add("0x00000003", "bug-check-0x3--invalid-affinity-set");
            documentation.Add("0x00000004", "bug-check-0x4--invalid-data-access-trap");
            documentation.Add("0x00000005", "bug-check-0x5--invalid-process-attach-attempt");
            documentation.Add("0x00000006", "bug-check-0x6--invalid-process-detach-attempt");
            documentation.Add("0x00000007", "bug-check-0x7--invalid-software-interrupt");
            documentation.Add("0x00000008", "bug-check-0x8--irql-not-dispatch-level");
            documentation.Add("0x00000009", "bug-check-0x9--irql-not-greater-or-equal");
            documentation.Add("0x0000000A", "bug-check-0xa--irql-not-less-or-equal");
            documentation.Add("0x0000000B", "bug-check-0xb--no-exception-handling-support");
            documentation.Add("0x0000000C", "bug-check-0xc--maximum-wait-objects-exceeded");
            documentation.Add("0x0000000D", "bug-check-0xd--mutex-level-number-violation");
            documentation.Add("0x0000000E", "bug-check-0xe--no-user-mode-context");
            documentation.Add("0x0000000F", "bug-check-0xf--spin-lock-already-owned");
            documentation.Add("0x00000010", "bug-check-0x10--spin-lock-not-owned");
            documentation.Add("0x00000011", "bug-check-0x11--thread-not-mutex-owner");
            documentation.Add("0x00000012", "bug-check-0x12--trap-cause-unknown");
            documentation.Add("0x00000013", "bug-check-0x13--empty-thread-reaper-list");
            documentation.Add("0x00000014", "bug-check-0x14--create-delete-lock-not-locked");
            documentation.Add("0x00000015", "bug-check-0x15--last-chance-called-from-kmode");
            documentation.Add("0x00000016", "bug-check-0x16--cid-handle-creation");
            documentation.Add("0x00000017", "bug-check-0x17--cid-handle-deletion");
            documentation.Add("0x00000018", "bug-check-0x18--reference-by-pointer");
            documentation.Add("0x00000019", "bug-check-0x19--bad-pool-header");
            documentation.Add("0x0000001A", "bug-check-0x1a--memory-management");
            documentation.Add("0x0000001B", "bug-check-0x1b--pfn-share-count");
            documentation.Add("0x0000001C", "bug-check-0x1c--pfn-reference-count");
            documentation.Add("0x0000001D", "bug-check-0x1d--no-spin-lock-available");
            documentation.Add("0x0000001E", "bug-check-0x1e--kmode-exception-not-handled");
            documentation.Add("0x0000001F", "bug-check-0x1f--shared-resource-conv-error");
            documentation.Add("0x00000020", "bug-check-0x20--kernel-apc-pending-during-exit");
            documentation.Add("0x00000021", "bug-check-0x21--quota-underflow");
            documentation.Add("0x00000022", "bug-check-0x22--file-system");
            documentation.Add("0x00000023", "bug-check-0x23--fat-file-system");
            documentation.Add("0x00000024", "bug-check-0x24--ntfs-file-system");
            documentation.Add("0x00000025", "bug-check-0x25--npfs-file-system");
            documentation.Add("0x00000026", "bug-check-0x26--cdfs-file-system");
            documentation.Add("0x00000027", "bug-check-0x27--rdr-file-system");
            documentation.Add("0x00000028", "bug-check-0x28--corrupt-access-token");
            documentation.Add("0x00000029", "bug-check-0x29--security-system");
            documentation.Add("0x0000002A", "bug-check-0x2a--inconsistent-irp");
            documentation.Add("0x0000002B", "bug-check-0x2b--panic-stack-switch");
            documentation.Add("0x0000002C", "bug-check-0x2c--port-driver-internal");
            documentation.Add("0x0000002D", "bug-check-0x2d--scsi-disk-driver-internal");
            documentation.Add("0x0000002E", "bug-check-0x2e--data-bus-error");
            documentation.Add("0x0000002F", "bug-check-0x2f--instruction-bus-error");
            documentation.Add("0x00000030", "bug-check-0x30--set-of-invalid-context");
            documentation.Add("0x00000031", "bug-check-0x31--phase0-initialization-failed");
            documentation.Add("0x00000032", "bug-check-0x32--phase1-initialization-failed");
            documentation.Add("0x00000033", "bug-check-0x33--unexpected-initialization-call");
            documentation.Add("0x00000034", "bug-check-0x34--cache-manager");
            documentation.Add("0x00000035", "bug-check-0x35--no-more-irp-stack-locations");
            documentation.Add("0x00000036", "bug-check-0x36--device-reference-count-not-zero");
            documentation.Add("0x00000037", "bug-check-0x37--floppy-internal-error");
            documentation.Add("0x00000038", "bug-check-0x38--serial-driver-internal");
            documentation.Add("0x00000039", "bug-check-0x39--system-exit-owned-mutex");
            documentation.Add("0x0000003A", "bug-check-0x3a--system-unwind-previous-user");
            documentation.Add("0x0000003B", "bug-check-0x3b--system-service-exception");
            documentation.Add("0x0000003C", "bug-check-0x3c--interrupt-unwind-attempted");
            documentation.Add("0x0000003D", "bug-check-0x3d--interrupt-exception-not-handled");
            documentation.Add("0x0000003E", "bug-check-0x3e--multiprocessor-configuration-not-supported");
            documentation.Add("0x0000003F", "bug-check-0x3f--no-more-system-ptes");
            documentation.Add("0x00000040", "bug-check-0x40--target-mdl-too-small");
            documentation.Add("0x00000041", "bug-check-0x41--must-succeed-pool-empty");
            documentation.Add("0x00000042", "bug-check-0x42--atdisk-driver-internal");
            documentation.Add("0x00000043", "bug-check-0x43--no-such-partition");
            documentation.Add("0x00000044", "bug-check-0x44--multiple-irp-complete-requests");
            documentation.Add("0x00000045", "bug-check-0x45--insufficient-system-map-regs");
            documentation.Add("0x00000046", "bug-check-0x46--deref-unknown-logon-session");
            documentation.Add("0x00000047", "bug-check-0x47--ref-unknown-logon-session");
            documentation.Add("0x00000048", "bug-check-0x48--cancel-state-in-completed-irp");
            documentation.Add("0x00000049", "bug-check-0x49--page-fault-with-interrupts-off");
            documentation.Add("0x0000004A", "bug-check-0x4a--irql-gt-zero-at-system-service");
            documentation.Add("0x0000004B", "bug-check-0x4b--streams-internal-error");
            documentation.Add("0x0000004C", "bug-check-0x4c--fatal-unhandled-hard-error");
            documentation.Add("0x0000004D", "bug-check-0x4d--no-pages-available");
            documentation.Add("0x0000004E", "bug-check-0x4e--pfn-list-corrupt");
            documentation.Add("0x0000004F", "bug-check-0x4f--ndis-internal-error");
            documentation.Add("0x00000050", "bug-check-0x50--page-fault-in-nonpaged-area");
            documentation.Add("0x00000051", "bug-check-0x51--registry-error");
            documentation.Add("0x00000052", "bug-check-0x52--mailslot-file-system");
            documentation.Add("0x00000053", "bug-check-0x53--no-boot-device");
            documentation.Add("0x00000054", "bug-check-0x54--lm-server-internal-error");
            documentation.Add("0x00000055", "bug-check-0x55--data-coherency-exception");
            documentation.Add("0x00000056", "bug-check-0x56--instruction-coherency-exception");
            documentation.Add("0x00000057", "bug-check-0x57--xns-internal-error");
            documentation.Add("0x00000058", "bug-check-0x58--ftdisk-internal-error");
            documentation.Add("0x00000059", "bug-check-0x59--pinball-file-system");
            documentation.Add("0x0000005A", "bug-check-0x5a--critical-service-failed");
            documentation.Add("0x0000005B", "bug-check-0x5b--set-env-var-failed");
            documentation.Add("0x0000005C", "bug-check-0x5c--hal-initialization-failed");
            documentation.Add("0x0000005D", "bug-check-0x5d--unsupported-processor");
            documentation.Add("0x0000005E", "bug-check-0x5e--object-initialization-failed");
            documentation.Add("0x0000005F", "bug-check-0x5f--security-initialization-failed");
            documentation.Add("0x00000060", "bug-check-0x60--process-initialization-failed");
            documentation.Add("0x00000061", "bug-check-0x61--hal1-initialization-failed");
            documentation.Add("0x00000062", "bug-check-0x62--object1-initialization-failed");
            documentation.Add("0x00000063", "bug-check-0x63--security1-initialization-failed");
            documentation.Add("0x00000064", "bug-check-0x64--symbolic-initialization-failed");
            documentation.Add("0x00000065", "bug-check-0x65--memory1-initialization-failed");
            documentation.Add("0x00000066", "bug-check-0x66--cache-initialization-failed");
            documentation.Add("0x00000067", "bug-check-0x67--config-initialization-failed");
            documentation.Add("0x00000068", "bug-check-0x68--file-initialization-failed");
            documentation.Add("0x00000069", "bug-check-0x69--io1-initialization-failed");
            documentation.Add("0x0000006A", "bug-check-0x6a--lpc-initialization-failed");
            documentation.Add("0x0000006B", "bug-check-0x6b--process1-initialization-failed");
            documentation.Add("0x0000006C", "bug-check-0x6c--refmon-initialization-failed");
            documentation.Add("0x0000006D", "bug-check-0x6d--session1-initialization-failed");
            documentation.Add("0x0000006E", "bug-check-0x6e--session2-initialization-failed");
            documentation.Add("0x0000006F", "bug-check-0x6f--session3-initialization-failed");
            documentation.Add("0x00000070", "bug-check-0x70--session4-initialization-failed");
            documentation.Add("0x00000071", "bug-check-0x71--session5-initialization-failed");
            documentation.Add("0x00000072", "bug-check-0x72--assign-drive-letters-failed");
            documentation.Add("0x00000073", "bug-check-0x73--config-list-failed");
            documentation.Add("0x00000074", "bug-check-0x74--bad-system-config-info");
            documentation.Add("0x00000075", "bug-check-0x75--cannot-write-configuration");
            documentation.Add("0x00000076", "bug-check-0x76--process-has-locked-pages");
            documentation.Add("0x00000077", "bug-check-0x77--kernel-stack-inpage-error");
            documentation.Add("0x00000078", "bug-check-0x78--phase0-exception");
            documentation.Add("0x00000079", "bug-check-0x79--mismatched-hal");
            documentation.Add("0x0000007A", "bug-check-0x7a--kernel-data-inpage-error");
            documentation.Add("0x0000007B", "bug-check-0x7b--inaccessible-boot-device");
            documentation.Add("0x0000007C", "bug-check-0x7c--bugcode-ndis-driver");
            documentation.Add("0x0000007D", "bug-check-0x7d--install-more-memory");
            documentation.Add("0x0000007E", "bug-check-0x7e--system-thread-exception-not-handled");
            documentation.Add("0x0000007F", "bug-check-0x7f--unexpected-kernel-mode-trap");
            documentation.Add("0x00000080", "bug-check-0x80--nmi-hardware-failure");
            documentation.Add("0x00000081", "bug-check-0x81--spin-lock-init-failure");
            documentation.Add("0x00000082", "bug-check-0x82--dfs-file-system");
            documentation.Add("0x00000085", "bug-check-0x85--setup-failure");
            documentation.Add("0x0000008B", "bug-check-0x8b--mbr-checksum-mismatch");
            documentation.Add("0x0000008E", "bug-check-0x8e--kernel-mode-exception-not-handled");
            documentation.Add("0x0000008F", "bug-check-0x8f--pp0-initialization-failed");
            documentation.Add("0x00000090", "bug-check-0x90--pp1-initialization-failed");
            documentation.Add("0x00000092", "bug-check-0x92--up-driver-on-mp-system");
            documentation.Add("0x00000093", "bug-check-0x93--invalid-kernel-handle");
            documentation.Add("0x00000094", "bug-check-0x94--kernel-stack-locked-at-exit");
            documentation.Add("0x00000096", "bug-check-0x96--invalid-work-queue-item");
            documentation.Add("0x00000097", "bug-check-0x97--bound-image-unsupported");
            documentation.Add("0x00000098", "bug-check-0x98--end-of-nt-evaluation-period");
            documentation.Add("0x00000099", "bug-check-0x99--invalid-region-or-segment");
            documentation.Add("0x0000009A", "bug-check-0x9a--system-license-violation");
            documentation.Add("0x0000009B", "bug-check-0x9b--udfs-file-system");
            documentation.Add("0x0000009C", "bug-check-0x9c--machine-check-exception");
            documentation.Add("0x0000009E", "bug-check-0x9e--user-mode-health-monitor");
            documentation.Add("0x0000009F", "bug-check-0x9f--driver-power-state-failure");
            documentation.Add("0x000000A0", "bug-check-0xa0--internal-power-error");
            documentation.Add("0x000000A1", "bug-check-0xa1--pci-bus-driver-internal");
            documentation.Add("0x000000A2", "bug-check-0xa2--memory-image-corrupt");
            documentation.Add("0x000000A3", "bug-check-0xa3--acpi-driver-internal");
            documentation.Add("0x000000A4", "bug-check-0xa4--cnss-file-system-filter");
            documentation.Add("0x000000A5", "bug-check-0xa5--acpi-bios-error");
            documentation.Add("0x000000A7", "bug-check-0xa7--bad-exhandle");
            documentation.Add("0x000000AC", "bug-check-0xac--hal-memory-allocation");
            documentation.Add("0x000000AD", "bug-check-0xad--video-driver-debug-report-request");
            documentation.Add("0x000000B1", "bug-check-0xb1--bgi-detected-violation");
            documentation.Add("0x000000B4", "bug-check-0xb4--video-driver-init-failure");
            documentation.Add("0x000000B8", "bug-check-0xb8--attempted-switch-from-dpc");
            documentation.Add("0x000000B9", "bug-check-0xb9--chipset-detected-error");
            documentation.Add("0x000000BA", "bug-check-0xba--session-has-valid-views-on-exit");
            documentation.Add("0x000000BB", "bug-check-0xbb--network-boot-initialization-failed");
            documentation.Add("0x000000BC", "bug-check-0xbc--network-boot-duplicate-address");
            documentation.Add("0x000000BD", "bug-check-0xbd--invalid-hibernated-state");
            documentation.Add("0x000000BE", "bug-check-0xbe--attempted-write-to-readonly-memory");
            documentation.Add("0x000000BF", "bug-check-0xbf--mutex-already-owned");
            documentation.Add("0x000000C1", "bug-check-0xc1--special-pool-detected-memory-corruption");
            documentation.Add("0x000000C2", "bug-check-0xc2--bad-pool-caller");
            documentation.Add("0x000000C4", "bug-check-0xc4--driver-verifier-detected-violation");
            documentation.Add("0x000000C5", "bug-check-0xc5--driver-corrupted-expool");
            documentation.Add("0x000000C6", "bug-check-0xc6--driver-caught-modifying-freed-pool");
            documentation.Add("0x000000C7", "bug-check-0xc7--timer-or-dpc-invalid");
            documentation.Add("0x000000C8", "bug-check-0xc8--irql-unexpected-value");
            documentation.Add("0x000000C9", "bug-check-0xc9--driver-verifier-iomanager-violation");
            documentation.Add("0x000000CA", "bug-check-0xca--pnp-detected-fatal-error");
            documentation.Add("0x000000CB", "bug-check-0xcb--driver-left-locked-pages-in-process");
            documentation.Add("0x000000CC", "bug-check-0xcc--page-fault-in-freed-special-pool");
            documentation.Add("0x000000CD", "bug-check-0xcd--page-fault-beyond-end-of-allocation");
            documentation.Add("0x000000CE", "bug-check-0xce--driver-unloaded-without-cancelling-pending-operations");
            documentation.Add("0x000000CF", "bug-check-0xcf--terminal-server-driver-made-incorrect-memory-reference");
            documentation.Add("0x000000D0", "bug-check-0xd0--driver-corrupted-mmpool");
            documentation.Add("0x000000D1", "bug-check-0xd1--driver-irql-not-less-or-equal");
            documentation.Add("0x000000D2", "bug-check-0xd2--bugcode-id-driver");
            documentation.Add("0x000000D3", "bug-check-0xd3--driver-portion-must-be-nonpaged");
            documentation.Add("0x000000D4", "bug-check-0xd4--system-scan-at-raised-irql-caught-improper-driver-unlo");
            documentation.Add("0x000000D5", "bug-check-0xd5--driver-page-fault-in-freed-special-pool");
            documentation.Add("0x000000D6", "bug-check-0xd6--driver-page-fault-beyond-end-of-allocation");
            documentation.Add("0x000000D7", "bug-check-0xd7--driver-unmapping-invalid-view");
            documentation.Add("0x000000D8", "bug-check-0xd8--driver-used-excessive-ptes");
            documentation.Add("0x000000D9", "bug-check-0xd9--locked-pages-tracker-corruption");
            documentation.Add("0x000000DA", "bug-check-0xda--system-pte-misuse");
            documentation.Add("0x000000DB", "bug-check-0xdb--driver-corrupted-sysptes");
            documentation.Add("0x000000DC", "bug-check-0xdc--driver-invalid-stack-access");
            documentation.Add("0x000000DE", "bug-check-0xde--pool-corruption-in-file-area");
            documentation.Add("0x000000DF", "bug-check-0xdf--impersonating-worker-thread");
            documentation.Add("0x000000E0", "bug-check-0xe0--acpi-bios-fatal-error");
            documentation.Add("0x000000E1", "bug-check-0xe1--worker-thread-returned-at-bad-irql");
            documentation.Add("0x000000E2", "bug-check-0xe2--manually-initiated-crash");
            documentation.Add("0x000000E3", "bug-check-0xe3--resource-not-owned");
            documentation.Add("0x000000E4", "bug-check-0xe4--worker-invalid");
            documentation.Add("0x000000E6", "bug-check-0xe6--driver-verifier-dma-violation");
            documentation.Add("0x000000E7", "bug-check-0xe7--invalid-floating-point-state");
            documentation.Add("0x000000E8", "bug-check-0xe8--invalid-cancel-of-file-open");
            documentation.Add("0x000000E9", "bug-check-0xe9--active-ex-worker-thread-termination");
            documentation.Add("0x000000EA", "bug-check-0xea--thread-stuck-in-device-driver");
            documentation.Add("0x000000EB", "bug-check-0xeb--dirty-mapped-pages-congestion");
            documentation.Add("0x000000EC", "bug-check-0xec--session-has-valid-special-pool-on-exit");
            documentation.Add("0x000000ED", "bug-check-0xed--unmountable-boot-volume");
            documentation.Add("0x000000EF", "bug-check-0xef--critical-process-died");
            documentation.Add("0x000000F0", "bug-check-0xf0--storage-miniport-error");
            documentation.Add("0x000000F1", "bug-check-0xf1--scsi-verifier-detected-violation");
            documentation.Add("0x000000F2", "bug-check-0xf2--hardware-interrupt-storm");
            documentation.Add("0x000000F3", "bug-check-0xf3--disorderly-shutdown");
            documentation.Add("0x000000F4", "bug-check-0xf4--critical-object-termination");
            documentation.Add("0x000000F5", "bug-check-0xf5--fltmgr-file-system");
            documentation.Add("0x000000F6", "bug-check-0xf6--pci-verifier-detected-violation");
            documentation.Add("0x000000F7", "bug-check-0xf7--driver-overran-stack-buffer");
            documentation.Add("0x000000F8", "bug-check-0xf8--ramdisk-boot-initialization-failed");
            documentation.Add("0x000000F9", "bug-check-0xf9--driver-returned-status-reparse-for-volume-open");
            documentation.Add("0x000000FA", "bug-check-0xfa---http-driver-corrupted");
            documentation.Add("0x000000FC", "bug-check-0xfc---attempted-execute-of-noexecute-memory");
            documentation.Add("0x000000FD", "bug-check-0xfd---dirty-nowrite-pages-congestion");
            documentation.Add("0x000000FE", "bug-check-0xfe--bugcode-usb-driver");
            documentation.Add("0x000000FF", "bug-check-0xff---reserve-queue-overflow");
            documentation.Add("0x00000100", "bug-check-0x100---loader-block-mismatch");
            documentation.Add("0x00000101", "bug-check-0x101---clock-watchdog-timeout");
            documentation.Add("0x00000102", "bug-check-0x102--dpc-watchdog-timeout");
            documentation.Add("0x00000103", "bug-check-0x103---mup-file-system");
            documentation.Add("0x00000104", "bug-check-0x104---agp-invalid-access");
            documentation.Add("0x00000105", "bug-check-0x105---agp-gart-corruption");
            documentation.Add("0x00000106", "bug-check-0x106---agp-illegally-reprogrammed");
            documentation.Add("0x00000108", "bug-check-0x108--third-party-file-system-failure");
            documentation.Add("0x00000109", "bug-check-0x109---critical-structure-corruption");
            documentation.Add("0x0000010A", "bug-check-0x10a---app-tagging-initialization-failed");
            documentation.Add("0x0000010C", "bug-check-0x10c---fsrtl-extra-create-parameter-violation");
            documentation.Add("0x0000010D", "bug-check-0x10d---wdf-violation");
            documentation.Add("0x0000010E", "bug-check-0x10e---video-memory-management-internal");
            documentation.Add("0x0000010F", "bug-check-0x10f---resource-manager-exception-not-handled");
            documentation.Add("0x00000111", "bug-check-0x111---recursive-nmi");
            documentation.Add("0x00000112", "bug-check-0x112---msrpc-state-violation");
            documentation.Add("0x00000113", "bug-check-0x113---video-dxgkrnl-fatal-error");
            documentation.Add("0x00000114", "bug-check-0x114---video-shadow-driver-fatal-error");
            documentation.Add("0x00000115", "bug-check-0x115---agp-internal");
            documentation.Add("0x00000116", "bug-check-0x116---video-tdr-failure");
            documentation.Add("0x00000117", "bug-check-0x117---video-tdr-timeout-detected");
            documentation.Add("0x00000119", "bug-check-0x119---video-scheduler-internal-error");
            documentation.Add("0x0000011A", "bug-check-0x11a---em-initialization-failure");
            documentation.Add("0x0000011B", "bug-check-0x11b---driver-returned-holding-cancel-lock");
            documentation.Add("0x0000011C", "bug-check-0x11c--attempted-write-to-cm-protected-storage");
            documentation.Add("0x0000011D", "bug-check-0x11d---event-tracing-fatal-error");
            documentation.Add("0x0000011E", "bug-check-0x11e--too-many-recursive-faults");
            documentation.Add("0x0000011F", "bug-check-0x11f--invalid-driver-handle");
            documentation.Add("0x00000120", "bug-check-0x120--bitlocker-fatal-error-");
            documentation.Add("0x00000121", "bug-check-0x121---driver-violation");
            documentation.Add("0x00000122", "bug-check-0x122---whea-internal-error");
            documentation.Add("0x00000123", "bug-check-0x123--crypto-self-test-failure-");
            documentation.Add("0x00000124", "bug-check-0x124---whea-uncorrectable-error");
            documentation.Add("0x00000125", "bug-check-0x125--nmr-invalid-state");
            documentation.Add("0x00000126", "bug-check-0x126--netio-invalid-pool-caller");
            documentation.Add("0x00000127", "bug-check-0x127---page-not-zero");
            documentation.Add("0x00000128", "bug-check-0x128--worker-thread-returned-with-bad-io-priority");
            documentation.Add("0x00000129", "bug-check-0x129--worker-thread-returned-with-bad-paging-io-priority");
            documentation.Add("0x0000012A", "bug-check-0x12a--mui-no-valid-system-language");
            documentation.Add("0x0000012B", "bug-check-0x12b---faulty-hardware-corrupted-page");
            documentation.Add("0x0000012C", "bug-check-0x12c---exfat-file-system");
            documentation.Add("0x0000012D", "bug-check-0x12d--volsnap-overlapped-table-access");
            documentation.Add("0x0000012E", "bug-check-0x12e--invalid-mdl-range");
            documentation.Add("0x0000012F", "bug-check-0x12f--vhd-boot-initialization-failed");
            documentation.Add("0x00000130", "bug-check-0x130--dynamic-add-processor-mismatch");
            documentation.Add("0x00000131", "bug-check-0x131--invalid-extended-processor-state");
            documentation.Add("0x00000132", "bug-check-0x132--resource-owner-pointer-invalid");
            documentation.Add("0x00000133", "bug-check-0x133-dpc-watchdog-violation");
            documentation.Add("0x00000134", "bug-check-0x134--drive-extender");
            documentation.Add("0x00000135", "bug-check-0x135--registry-filter-driver-exception");
            documentation.Add("0x00000136", "bug-check-0x136--vhd-boot-host-volume-not-enough-space");
            documentation.Add("0x00000137", "bug-check-0x137--win32k-handle-manager");
            documentation.Add("0x00000138", "bug-check-0x138-gpio-controller-driver-error");
            documentation.Add("0x00000139", "bug-check-0x139--kernel-security-check-failure");
            documentation.Add("0x0000013A", "bug-check-0x13a--kernel-mode-heap-corruption");
            documentation.Add("0x0000013B", "bug-check-0x13b--passive-interrupt-error");
            documentation.Add("0x0000013C", "bug-check-0x13c--invalid-io-boost-state");
            documentation.Add("0x0000013D", "bug-check-0x13d--critical-initialization-failure");
            documentation.Add("0x00000140", "bug-check-0x140--storage-device-abnormality-detected");
            documentation.Add("0x00000143", "bug-check-0x143--processor-driver-internal");
            documentation.Add("0x00000144", "bug-check-0x144--bugcode-usb3-driver");
            documentation.Add("0x00000145", "bug-check-0x145--secure-boot-violation-");
            documentation.Add("0x00000147", "bug-check-0x147--abnormal-reset-detected");
            documentation.Add("0x00000149", "bug-check-0x149--refs-file-system");
            documentation.Add("0x0000014A", "bug-check-0x14a--kernel-wmi-internal");
            documentation.Add("0x0000014B", "bug-check-0x14b--soc-subsystem-failure");
            documentation.Add("0x0000014C", "bug-check-0x14c--fatal-abnormal-reset-error");
            documentation.Add("0x0000014D", "bug-check-0x14d--exception-scope-invalid");
            documentation.Add("0x0000014E", "bug-check-0x14e--soc-critical-device-removed");
            documentation.Add("0x0000014F", "bug-check-0x14f--pdc-watchdog-timeout");
            documentation.Add("0x00000150", "bug-check-0x150--tcpip-aoac-nic-active-reference-leak");
            documentation.Add("0x00000151", "bug-check-0x151--unsupported-instruction-mode");
            documentation.Add("0x00000152", "bug-check-0x152--invalid-push-lock-flags");
            documentation.Add("0x00000153", "bug-check-0x153--kernel-lock-entry-leaked-on-thread-termination");
            documentation.Add("0x00000154", "bug-check-0x154--unexpected-store-exception");
            documentation.Add("0x00000155", "bug-check-0x155--os-data-tampering");
            documentation.Add("0x00000157", "bug-check-0x157--kernel-thread-priority-floor-violation");
            documentation.Add("0x00000158", "bug-check-0x158--illegal-iommu-page-fault");
            documentation.Add("0x00000159", "bug-check-0x159--hal-illegal-iommu-page-fault");
            documentation.Add("0x0000015A", "bug-check-0x15a--sdbus-internal-error");
            documentation.Add("0x0000015B", "bug-check-0x15b--worker-thread-returned-with-system-page-priority-active");
            documentation.Add("0x00000160", "bug-check-0x160--win32k-atomic-check-failure");
            documentation.Add("0x00000162", "bug-check-0x162--kernel-auto-boost-invalid-lock-release");
            documentation.Add("0x00000163", "bug-check-0x162--worker-thread-test-condition");
            documentation.Add("0x00000164", "bug-check-0x164--win32k-critical-failure");
            documentation.Add("0x0000016C", "bug-check-0x16c--invalid-rundown-protection-flags");
            documentation.Add("0x0000016D", "bug-check-0x16d--invalid-slot-allocator-flags");
            documentation.Add("0x0000016E", "bug-check-0x16e--eresource-invalid-release");
            documentation.Add("0x00000170", "bug-check-0x170--cluster-csv-clussvc-disconnect-watchdog");
            documentation.Add("0x00000171", "bug-check-0x171--crypto-library-internal-error");
            documentation.Add("0x00000173", "bug-check-0x173--coremsgcall-internal-error");
            documentation.Add("0x00000174", "bug-check-0x174--coremsg-internal-error");
            documentation.Add("0x00000178", "bug-check-0x178--elam-driver-detected-fatal-error");
            documentation.Add("0x0000017B", "bug-check-0x17b--profiler-configuration-illegal");
            documentation.Add("0x0000017E", "bug-check-0x17e--microcode-revision-mismatch");
            documentation.Add("0x00000187", "bug-check-0x187--video-dwminit-timeout-fallback-bdd");
            documentation.Add("0x00000189", "bug-check-0x189--bad-object-header");
            documentation.Add("0x0000018B", "bug-check-0x18b--secure-kernel-error");
            documentation.Add("0x0000018C", "bug-check-0x18c--hyperguard-violation");
            documentation.Add("0x0000018D", "bug-check-0x18d--secure-fault-unhandled");
            documentation.Add("0x0000018E", "bug-check-0x18e--kernel-partition-reference-violation");
            documentation.Add("0x00000191", "bug-check-0x191--pf-detected-corruption");
            documentation.Add("0x00000192", "bug-check-0x192--kernel-auto-boost-lock-acquisition-with-raised-irql");
            documentation.Add("0x00000196", "bug-check-0x196--loader-rollback-detected");
            documentation.Add("0x00000197", "bug-check-0x197--win32k-security-failure");
            documentation.Add("0x00000199", "bug-check-0x199--kernel-storage-slot-in-use");
            documentation.Add("0x0000019A", "bug-check-0x19a--worker-thread-returned-while-attached-to-silo");
            documentation.Add("0x0000019B", "bug-check-0x19b--ttm-fatal-error");
            documentation.Add("0x0000019C", "bug-check-0x19c--win32k-power-watchdog-timeout");
            documentation.Add("0x000001A0", "bug-check-0x1a0--ttm-watchdog-timeout");
            documentation.Add("0x000001A2", "bug-check-0x1a2--win32k-callout-watchdog-bugcheck");
            documentation.Add("0x000001AA", "bug-check-0x1aa-exception-on-invalid-stack");
            documentation.Add("0x000001AB", "bug-check-0x1ab-unwind-on-invalid-stack");
            documentation.Add("0x000001C6", "bug-check-0x1c6--fast-eresource-precondition-violation");
            documentation.Add("0x000001C7", "bug-check-0x1c7--store-data-structure-corruption");
            documentation.Add("0x000001C8", "bug-check-0x1c8--manually-initiated-power-button-hold");
            documentation.Add("0x000001CA", "bug-check-0x1ca--synthetic-watchdog-timeout");
            documentation.Add("0x000001CB", "bug-check-0x1cb--invalid-silo-detach");
            documentation.Add("0x000001CD", "bug-check-0x1cd--invalid-callback-stack-address");
            documentation.Add("0x000001CE", "bug-check-0x1ce--invalid-kernel-stack-address");
            documentation.Add("0x000001CF", "bug-check-0x1cf--hardware-watchdog-timeout");
            documentation.Add("0x000001D0", "bug-check-0x1d0--acpi-firmware-watchdog-timeout");
            documentation.Add("0x000001D2", "bug-check-0x1d2--worker-thread-invalid-state");
            documentation.Add("0x000001D3", "bug-check-0x1d3--wfp-invalid-operation");
            documentation.Add("0x000001D5", "bug-check-0x1d5--driver-pnp-watchdog");
            documentation.Add("0x000001D6", "bug-check-0x1d6--worker-thread-returned-with-non-default-workload-class");
            documentation.Add("0x000001D7", "bug-check-0x1d7--efs-fatal-error");
            documentation.Add("0x000001D8", "bug-check-0x1d8--ucmucsi-failure");
            documentation.Add("0x000001D9", "bug-check-0x1d8--ucmucsi-failure");
            documentation.Add("0x000001DA", "bug-check-0x1da--hal-blocked-processor-internal-error");
            documentation.Add("0x000001DB", "bug-check-0x1db--ipi-watchdog-timeout");
            documentation.Add("0x000001DC", "bug-check-0x1dc--dma-common-buffer-vector-error");
            documentation.Add("0x000001DD", "bug-check-0x1dd--bugcode-mbbadapter-driver");
            documentation.Add("0x000001DE", "bug-check-0x1de--bugcode-wifiadapter-driver");
            documentation.Add("0x000001E4", "bug-check-0x1e4--video-dxgkrnl-sysmm-fatal-error");
            documentation.Add("0x000001E9", "bug-check-0x1e9--illegal-ats-initialization");
            documentation.Add("0x000001EA", "bug-check-0x1ea--secure-pci-config-space-access-violation");
            documentation.Add("0x000001EB", "bug-check-0x1eb--dam-watchdog-timeout");
            documentation.Add("0x000001ED", "bug-check-0x1ed--handle-error-on-critical-thread");
            documentation.Add("0x00000356", "bug-check-0x356--xbox-eractrl-cs-timeout");
            documentation.Add("0x00000BFE", "bug-check-0xbfe--bc-bluetooth-verifier-fault");
            documentation.Add("0x00000BFF", "bug-check-0xbff--bc-bthmini-verifier-fault");
            documentation.Add("0x00020001", "bug-check-0x20001--hypervisor-error");
            documentation.Add("0x1000007E", "bug-check-0x1000007e--system-thread-exception-not-handled-m");
            documentation.Add("0x1000007F", "bug-check-0x1000007f--unexpected-kernel-mode-trap-m");
            documentation.Add("0x1000008E", "bug-check-0x1000008e--kernel-mode-exception-not-handled-m");
            documentation.Add("0x100000EA", "bug-check-0x100000ea--thread-stuck-in-device-driver-m");
            documentation.Add("0x4000008A", "bug-check-0x4000008a--thread-terminate-held-mutex");
            documentation.Add("0xC0000218", "bug-check-0xc0000218--status-cannot-load-registry-file");
            documentation.Add("0xC000021A", "bug-check-0xc000021a--winlogin-fatal-error");
            documentation.Add("0xC0000221", "bug-check-0xc0000221--status-image-checksum-mismatch");
            documentation.Add("0xDEADDEAD", "bug-check-0xdeaddead--manually-initiated-crash1");
        }

        private void openDocsButton_Click(object sender, EventArgs e)
        {
            if (currentCrash == null) System.Diagnostics.Process.Start("https://learn.microsoft.com/en-us/windows-hardware/drivers/debugger/bug-check-code-reference2");
            else System.Diagnostics.Process.Start("https://learn.microsoft.com/en-us/windows-hardware/drivers/debugger/" + documentation[currentCrash.crashType]);
        }
    }
    public class Crash
    {
        public String crashType = "";
        public String[] parameters = new String[4];
        public String timestamp = "";
        public String message = "";
    }
}
