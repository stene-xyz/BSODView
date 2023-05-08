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
        Crash[] crashes;
        
        public void LoadEVTXFile(string filename)
        {
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
                                    if (dataItem.Attributes["Name"].InnerText == "BugcheckCode") crash.crashType = "0x" + string.Format("{0:X8}", int.Parse(dataItem.InnerText));
                                    if (dataItem.Attributes["Name"].InnerText == "BugcheckParameter1") crash.parameters[0] = "0x" + string.Format("{0:X8}", int.Parse(dataItem.InnerText.Substring(2)));
                                    if (dataItem.Attributes["Name"].InnerText == "BugcheckParameter2") crash.parameters[1] = "0x" + string.Format("{0:X8}", int.Parse(dataItem.InnerText.Substring(2)));
                                    if (dataItem.Attributes["Name"].InnerText == "BugcheckParameter3") crash.parameters[2] = "0x" + string.Format("{0:X8}", int.Parse(dataItem.InnerText.Substring(2)));
                                    if (dataItem.Attributes["Name"].InnerText == "BugcheckParameter4") crash.parameters[3] = "0x" + string.Format("{0:X8}", int.Parse(dataItem.InnerText.Substring(2)));
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
            CrashInfo.Items.Add("Crash at " + crashes[i].timestamp + ":");
            CrashInfo.Items.Add(translation[crashes[i].crashType] + " (bugcheck code " + crashes[i].crashType + ")");
            CrashInfo.Items.Add(crashes[i].message);
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
