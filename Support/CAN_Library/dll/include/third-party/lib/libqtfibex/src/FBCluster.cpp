#include "../inc/FRClusterType.h"
#include "../inc/FBCluster.h"

namespace ASAM {
namespace FIBEX {

FBCluster::FBCluster()
{
    /* nothing to do here */
}

FBCluster::~FBCluster()
{

}

QString FBCluster::name() const
{
    return m_frCluster?m_frCluster->m_shortName:QString();
}

} // FIBEX
} // ASAM
