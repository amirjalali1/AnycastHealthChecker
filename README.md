AnyCast HealthCheker
=====================

    A .Net Core HealthChecker for Anycasted Services
>The main idea of this Software and how it interacts with bird daemon is from [anycast_healthchecker](https://github.com/unixsurfer/anycast_healthchecker) but it is not a port of [Pavlos Parissis](https://github.com/unixsurfer) anycast_healthcheker
 

#### Contents
+ [Introduction](#Introduction)
+ [What is Anycast](#What-is-Anycast)
+ [How anycast-healthchecker works](#How-anycast-healthchecker-works)
+ [Configuring anycast-healthchecker](#Configuring-anycast-healthchecker)
  + [Bird configuration](#Bird-configuration)
+ [Contributors](#Contributors)
+ [Acknowledgement](#Acknowledgement)
+ [Contacts](#Contacts)




 

Introduction
-------------

**AnyCast HealthCheker** monitors a service by doing periodic health checks and, based on the result, instructing [Bird](https://bird.network.cz/) daemon to either advertise or withdraw the route to reach it. As a result Bird will only advertise routes for healthy services. Routes for both IPv4 and IPv6 addresses are supported.

This is the pre-release version of AnyCast HealthChecker and naming convetions are subject to change in the future.

You can easily extend the AnyCast HealthCheck by adding a monitor.

Monitors are configurable through a json file aoosettings.json and easy to develope by extending ISnapshotManager .

There are currently five monitors, Network Bandwitch monitor, Cpu usage monitor,Memory  usage monitor,Nginx availabilty monitor,Nginx responsiveness monitor

Bird must be configured in a certain way to interface properly with anycast-healthchecker. The configuration is detailed later in this document.

AnyCastHealthCheker is a .Net Core program, which runs in background.

What is Anycast
---------------

Anycast is a network addressing scheme where traffic from a sender has more than one potential receivers, but only one of them receives it.
Routing protocols decide which one of the potential receivers will actually receive traffic, according to the topology of the network. The main attribute contributing to this decision is the cost of the network path between a sender and a receiver.

Cost is a protocol specific value (usually an integer) that has meaning only within the domain of the protocol itself, and it is used as a metric of distance.
Routing protocols provide default values for common topologies ([BGP](https://en.wikipedia.org/wiki/Border_Gateway_Protocol) associates the cost of a path with the number of autonomous systems between the sender and the receiver, [[OSPF](https://en.wikipedia.org/wiki/Open_Shortest_Path_First) calculates the default cost based on the bandwidth of links), but its main use is to allow administrative control over traffic flow by specifying a cost according to business needs.

The closest receiver to a sender always receives the traffic; this changes only if something changes on the network, i.e. another receiver with a better path to the sender shows up or the current receiver disappears. If multiple receivers share the same distance from the sender, more than one might receive traffic, depending on how the routing protocol is configured.

The three pictures below show how traffic is routed between a sender and multiple potential receivers when something changes on network. In this example BGP routing protocol is used:

![alt text](https://github.com/amirjalali1/AnycastHealthChecker/raw/health-monitor/anycast-receivers-example1.png "anycast-receivers-example1.png")
![alt text](https://github.com/amirjalali1/AnycastHealthChecker/raw/health-monitor/anycast-receivers-example2.png "anycast-receivers-example2.png")
![alt text](https://github.com/amirjalali1/AnycastHealthChecker/raw/health-monitor/anycast-receivers-example3.png "anycast-receivers-example3.png")


These potential receivers use [BGP](https://en.wikipedia.org/wiki/Border_Gateway_Protocol) or [OSPF](https://en.wikipedia.org/wiki/Open_Shortest_Path_First) and simultaneously announce the same destination IP address from different places on the network. Due to the nature of Anycast, receivers can be located on any location across a global
network infrastructure.

Anycast doesn't balance traffic, as only one receiver attracts traffic from senders. For instance, if there are two receivers announcing the same destination IP address in different locations, traffic will be distributed between these two receivers unevenly, as senders can be spread across the network in an uneven way.

Anycast is being used as a mechanism to switch traffic between and within data-centers for the following main reasons:

* the switch of traffic occurs without the need to enforce a change on clients

In case of a service failure in one location, traffic to that location will be switched to another data-center without any manual intervention and, most importantly, without pushing a change to clients, which you don't have always
control on.

* the switch happens within few milliseconds

The same technology can be used for balancing traffic using [Equal-cost multi-path routing](https://en.wikipedia.org/wiki/Equal-cost_multi-path_routing).

ECMP routing is a network technology where traffic can be routed over multiple paths. In the context of routing protocols, path is the route a packet has to take in order to be delivered to a destination. Because these multiple paths share the same cost, traffic is balanced across them.

This grants the possibility to perform traffic load-balancing across multiple servers. Routers distribute traffic in a deterministic fashion, usually by selecting the next hop and looking at the following four properties of IP packets:

* source IP
* source PORT
* destination IP
* destination PORT

Each unique combination of these four properties is called network flow. For each different network flow a different destination is selected so that traffic is evenly balanced across all servers. These nodes run an Internet Routing software in the same way as in the Anycast case, but with the major difference that all servers receive traffic at the
same time.

The main characteristic of this type of load-balancing is that it is stateless. Router balances traffic to a destination IP address based on the quadruple network flow without the need to understand and inspect protocols above Layer3.
As a result, it is very cheap in terms of resources and very fast at the same time. This is commonly advertised as traffic balancing at "wire-speed".

**AnyCast HealthCheker** can be utilized in Anycast and ECMP environments.

How anycast-healthchecker works
-------------------------------

The current release of anycast-healthchecker supports only the Bird daemon, which has to be configured in a specific way. Therefore, it is useful to explain very briefly how Bird handles advertisements for routes.

Bird maintains a routing information base [RIB](https://en.wikipedia.org/wiki/Routing_table) and various protocols import/export routes to/from it. The diagram below illustrates how Bird advertises IP routes, assigned on the loopback interface, to the rest of the network using BGP protocol. Bird can also import routes learned via BGP/OSPF protocols, but this part of the routing process is irrelevant to the functionality of anycast-healthchecker.

![alt text](https://github.com/amirjalali1/AnycastHealthChecker/raw/health-monitor/bird_daemon_rib_explained.png "bird_daemon_rib_explained.png")

A route is always associated with a service that runs locally on the box. The Anycasted service is a daemon (HAProxy, Nginx, Bind etc) that processes incoming traffic and listens to an IP (Anycast Service Address) for which a route exists in the RIB and is advertised by Bird.

As shown in the above picture, a route is advertised only when:

#. The IP is assigned to the loopback interface.
#. [direct](https://bird.network.cz/?get_doc&f=bird-6.html#ss6.4) protocol from Bird imports a route for that IP in the RIB.
#. BGP/OSPF protocols export that route from the RIB to a network peer.

The route associated with the Anycasted service must be either advertised or withdrawn based on the health of the service, otherwise traffic will always be routed to the local node regardless of the status of the service.

Bird provides [filtering](https://bird.network.cz/?get_doc&f=bird-5.html) capabilities with the help of a simple programming language. A filter can be used to either accept or reject routes before they are exported from the RIB to the network.

A list of IP prefixes (\<IP>/\<prefix length>)  is stored in a text file. IP prefixes that **are not** included in the list are filtered-out and **are not** exported from the RIB to the network. The white-list text file is sourced by Bird upon startup, reload and reconfiguration. The following diagram illustrates how this technique works:

![alt text](https://github.com/amirjalali1/AnycastHealthChecker/raw/health-monitor/bird_daemon_filter_explained.png "bird_daemon_filter_explained.png")


This configuration logic allows a separate process to update the list by adding or removing IP prefixes and trigger a reconfiguration of Bird in order to advertise or withdraw routes.  **anycast-healthchecker** is that separate process. It monitors Anycasted services and, based on the status of the health checks, updates the list of IP prefixes.

Bird does not allow the definition of a list with no elements: if that happens Bird will produce an error and refuses to start. Because of this, anycast-healthchecker makes sure that there is always an IP prefix in the list, see ``dummy_ip_prefix`` and ``dummy_ip6_prefix`` settings in `Daemon section`_.

Configuring anycast-healthchecker
---------------------------------

Because anycast-healthchecker is very tied with with Bird daemon, the configuration of Bird has been explained first. Next, the configuration of anycast-healthchecker (including the configuration for the health checks) is covered and, finally, the options for invoking the program from the command line will be described.

IPv6 support
############

IPv4 and IPv6 addresses are supported by the Bird Internet Routing Daemon project by providing a different daemon per IP protocol version, bird for IPv4 and bird6 for IPv6. This implies that configuration files are split as well, meaning that you can't define IPv6 addresses in a configuration and source it by the IPv4 daemon.

Bird configuration
##################

The logic described in `How anycast-healthchecker works`_ can be accomplished by configuring:

#. an ``include`` statement to source other configuration files in
   ``bird.conf``
#. a function, ``match_route``, as an export filter for the routing
   protocol (BGP or OSPF)
#. a list of IP prefixes for routes which allowed to be exported by Bird

anycast-healthchecker **does not** install any of the aforementioned files.

bird.conf
*********

The most important parts are the lines ``include "/etc/bird.d/*.conf";`` and ``export where match_route();``. The former statement causes inclusion of other configuration files while the latter forces all routes to pass from the ``match_route`` function before they are exported. BGP protocol is used in the below example but OSPF protocol can be used as well::

    include "/etc/bird.d/*.conf";
    protocol device {
        scan time 10;
    }
    protocol direct direct1 {
        interface "lo";
            export none;
            import all;
    }
    template bgp bgp_peers {
        import none;
        export where match_route();
        local as 64815;
    }
    protocol bgp BGP1 from bgp_peers {
        disabled no;
        neighbor 10.248.7.254 as 64814;
    }

match-route.conf
****************

``match-route.conf`` file configures the ``match_route`` function, which performs the allow and deny of IP prefixes by looking at the IP prefix of the route in a list and exports it if it matches entry::

    function match_route()
    {
        return net ~ ACAST_PS_ADVERTISE;
    }

This is the equivalent function for IPv6::

    function match_route6()
    {
        return net ~ ACAST6_PS_ADVERTISE;
    }

anycast-prefixes.conf
*********************

``anycast-prefixes.conf`` file defines a list of IP prefixes which is stored in a variable named ``ACAST_PS_ADVERTISE``. The name of the variable can be anything meaningful but ``bird_variable`` setting **must** be changed accordingly.

::

    define ACAST_PS_ADVERTISE =
        [
            10.189.200.255/32
        ];

anycast-healthchecker removes IP prefixes from the list for which a service check is not configured. But, the IP prefix set in ``dummy_ip_prefix`` does not need a service check configuration.

This the equivalent list for IPv6 prefixes::

    define ACAST6_PS_ADVERTISE =
        [
            2001:db8::1/128
        ];

anycast-healthchecker creates ``anycast-prefixes.conf`` file for both IP versions upon startup if those file don't exist. After the launch **no other process(es) should** modify those files.

Use daemon settings ``bird_conf`` and ``bird6_conf`` to control the location of the files.

With the default settings those files are located under ``/var/lib/anycast-healthchecker`` and ``/var/lib/anycast-healthchecker/6``. Administrators must create those two directories with permissions ``755`` and user/group ownership to the account under which anycast-healthchecker runs.

Bird daemon loads configuration files by using the ``include`` statement in the main Bird configuration (`bird.conf`_). By default such ``include`` statement points to a directory under ``/etc/bird.d``, while ``anycast-prefixes.conf`` files are located under ``/var/lib/anycast-healthchecker`` directories. Therefore,
a link for each file must be created under ``/etc/bird.d`` directory. Administrators must also create those two links. Here is an example from a production server:

::

    % ls -ls /etc/bird.d/anycast-prefixes.conf
    4 lrwxrwxrwx 1 root root 105 Dec  2 16:08 /etc/bird.d/anycast-prefixes.conf ->
    /var/lib/anycast-healthchecker/anycast-prefixes.conf

    % ls -ls /etc/bird.d/6/anycast-prefixes.conf
    4 lrwxrwxrwx 1 root root 107 Jan 10 10:33 /etc/bird.d/6/anycast-prefixes.conf
    -> /var/lib/anycast-healthchecker/6/anycast-prefixes.conf

Contributors
---------------

The following people have contributed to project with feedback, commits and code reviews

- Amir Jalali ([@amirjalali1](https://github.com/amirjalali1))
- Mohammad Akbari ([@MohammadAkbari](https://github.com/MohammadAkbari))



Acknowledgement
---------------

The main idea of this Software and how it interacts with bird daemon is from [anycast_healthchecker](https://github.com/unixsurfer/anycast_healthchecker) but it is not a port of [Pavlos Parissis](https://github.com/unixsurfer) anycast_healthcheker

Contacts
--------

**Project website**: https://github.com/amirjalali1/AnycastHealthChecker

Amir Jalali <amir.jalali@gmail.com>