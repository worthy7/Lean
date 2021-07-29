# QuantConnect Python Algorithm Project

This document contains information regarding how to use Python with the Lean engine, this includes how to use Python Autocomplete, setting up Lean for Python algorithms, PythonNet compilation for devs, and what imports to use to replicate the web IDE experience in your local development.

<br />


------
# Local Python Autocomplete
To enable autocomplete for your local Python IDE, install the `quantconnect-stubs` package from PyPI using the following command:
```
pip install quantconnect-stubs
```

To update your autocomplete to the latest version, you can run the following command:
```
pip install --upgrade quantconnect-stubs
```

Copy and paste the imports found [here](#python-autocomplete-imports) to the top of your project file to enable autocomplete.

In addition, you can use [Skylight](https://www.quantconnect.com/skylight) to automatically sync local changes to the cloud.

<br />

------

# Setup Lean Locally with Python
Before setting up python support, follow the [installation instructions](https://github.com/QuantConnect/Lean#installation-instructions) to get LEAN running C# algorithms on your machine. 


## Installing Python 3.6:

Next we must prepare a Python installation for Lean to use. Follow the instructions for your OS.

<br />

### [Windows](https://github.com/QuantConnect/Lean#windows)

1. Use the Windows x86-64 MSI **Python 3.6.8** installer from [python.org](https://www.python.org/downloads/release/python-368/) or [Anaconda](https://repo.anaconda.com/archive/Anaconda3-5.2.0-Windows-x86_64.exe) for Windows installer. "Anaconda 5.2" installs 3.5.2 by default, after installation of Anaconda you will need to upgrade python to make it work as expected: `conda install -y python=3.6.8`
2. When asked to select the features to be installed, make sure you select "Add python.exe to Path"
3. Create `PYTHONNET_PYDLL` environment variable to the location of your python dll in your installation (e.g. `C:\Dev\Python368\python36.dll` or `C:\Anaconda3\python36.dll`):
   - Right mouse button on My Computer. Click Properties.
   - Click Advanced System Settings -> Environment Variables -> System Variables
   - Click **New**. 
        - Name: `PYTHONNET_PYDLL`
        - Value: `{python dll location}`
4. Install [pandas=0.25.3](https://pandas.pydata.org/) and its [dependencies](https://pandas.pydata.org/pandas-docs/stable/install.html#dependencies).
5. Install [wrapt=1.11.2](https://pypi.org/project/wrapt/) module.
6. Reboot computer to ensure changes are propagated.

<br />

### [macOS](https://github.com/QuantConnect/Lean#macos)

1. Use the macOS x86-64 package installer from [Anaconda](https://repo.anaconda.com/archive/Anaconda3-5.2.0-MacOSX-x86_64.pkg) and follow "[Installing on macOS](https://docs.anaconda.com/anaconda/install/mac-os)" instructions from Anaconda documentation page.
2. Set `PYTHONNET_PYDLL` environment variable to the location of your python dll in your installation directory (e.g. `/Users/{your_user_name}/anaconda3/lib/libpython3.6m.dylib`):
   - Open `~/.bash-profile` with a text editor of your choice.
   - Add a new line to the file containing 
   ```
   export PYTHONNET_PYDLL="/{your}/{path}/{here}/libpython3.6m.dylib"
   ```
   - Save your changes, and either restart your terminal *or* execute 
   ```
   source ~/.bash-profile
   ```
2. Install [pandas=0.25.3](https://pandas.pydata.org/) and its [dependencies](https://pandas.pydata.org/pandas-docs/stable/install.html#dependencies).
3. Install [wrapt=1.11.2](https://pypi.org/project/wrapt/) module.

<br />

### [Linux](https://github.com/QuantConnect/Lean#linux-debian-ubuntu)

1. Install Python using miniconda by following these commands; by default, **miniconda** is installed in the users home directory (`$HOME`):
```
export PATH="$HOME/miniconda3/bin:$PATH"
wget https://cdn.quantconnect.com/miniconda/Miniconda3-4.5.12-Linux-x86_64.sh
bash Miniconda3-4.5.12-Linux-x86_64.sh -b
rm -rf Miniconda3-4.5.12-Linux-x86_64.sh
conda update -y python conda pip
```
2. Create a new Python environment with the needed dependencies
```
conda create -n qc_lean python=3.6.8 cython=0.29.11 pandas=0.25.3 wrapt=1.11.2
```
3. Set `PYTHONNET_PYDLL` environment variable to location of your python dll in your installation directory (e.g. `/home/{your_user_name}/miniconda3/envs/qc_lean/lib/libpython3.6m.so`):
   - Open `/etc/environment` with a text editor of your choice.
   - Add a new line to the file containing 
   ```
   PYTHONNET_PYDLL="/home/{your_user_name}/miniconda3/envs/qc_lean/lib/libpython3.6m.so"
   ```
   - Save your changes, and logout or reboot to reflect these changes



<br />


## Run Python Algorithms

1. Update the [config](https://github.com/QuantConnect/Lean/blob/master/Launcher/config.json) to run a python algorithm:
    ```json
    "algorithm-type-name": "BasicTemplateAlgorithm",
    "algorithm-language": "Python",
    "algorithm-location": "../../../Algorithm.Python/BasicTemplateAlgorithm.py",
    ```
 2. Rebuild LEAN.
 3. Run LEAN. You should see the same result of the C# algorithm you tested earlier.

------

# Python.NET development - Python.Runtime.dll compilation

LEAN users do **not** need to compile `Python.Runtime.dll`. The information below is targeted to developers who wish to improve it. Download [QuantConnect/pythonnet](https://github.com/QuantConnect/pythonnet/) github clone or downloading the zip. If downloading the zip - unzip to a local pathway.

**Note:** QuantConnect's version of pythonnet is an enhanced version of [pythonnet](https://github.com/pythonnet/pythonnet) with added support for `System.Decimal` and `System.DateTime`.

Below are some examples of build commands that create a suitable `Python.Runtime.dll`.

```
msbuild pythonnet.sln /nologo /v:quiet /t:Clean;Rebuild 
```

OR

```
dotnet build pythonnet.sln
```

------

# Python Autocomplete Imports
Copy and paste these imports to the top of your Python file to enable a development experience equal to the cloud (these imports are exactly the same as the ones used in the QuantConnect Terminal).

```python
from QuantConnect import *
from QuantConnect.Parameters import *
from QuantConnect.Benchmarks import *
from QuantConnect.Brokerages import *
from QuantConnect.Util import *
from QuantConnect.Interfaces import *
from QuantConnect.Algorithm import *
from QuantConnect.Algorithm.Framework import *
from QuantConnect.Algorithm.Framework.Selection import *
from QuantConnect.Algorithm.Framework.Alphas import *
from QuantConnect.Algorithm.Framework.Portfolio import *
from QuantConnect.Algorithm.Framework.Execution import *
from QuantConnect.Algorithm.Framework.Risk import *
from QuantConnect.Indicators import *
from QuantConnect.Data import *
from QuantConnect.Data.Consolidators import *
from QuantConnect.Data.Custom import *
from QuantConnect.Data.Fundamental import *
from QuantConnect.Data.Market import *
from QuantConnect.Data.UniverseSelection import *
from QuantConnect.Notifications import *
from QuantConnect.Orders import *
from QuantConnect.Orders.Fees import *
from QuantConnect.Orders.Fills import *
from QuantConnect.Orders.Slippage import *
from QuantConnect.Scheduling import *
from QuantConnect.Securities import *
from QuantConnect.Securities.Equity import *
from QuantConnect.Securities.Forex import *
from QuantConnect.Securities.Interfaces import *
from datetime import date, datetime, timedelta
from QuantConnect.Python import *
from QuantConnect.Storage import *
QCAlgorithmFramework = QCAlgorithm
QCAlgorithmFrameworkBridge = QCAlgorithm
```

# Known Issues
- Python can sometimes have issues when paired with our quantconnect stubs package on Windows. This issue can cause modules not to be found because `site-packages` directory is not present in the python path. If you have the required modules installed and are seeing errors about them not being found, please try the following steps:
    - remove stubs -> pip uninstall quantconnect-stubs
    - reinstall stubs -> pip install quantconnect-stubs
