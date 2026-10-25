#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXSignalInstanceType.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType SIGNAL-INSTANCES
 *
 * A PDUs layout as a sequence of Signal-Instances
 */
class ASAM_FIBEX_EXPORT FXSignalInstances
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXSignalInstances(FXFibex *fibex);
    ~FXSignalInstances();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** element SIGNAL-INSTANCE */
    QHash<QString, FXSignalInstanceType*> m_sigInstances;
};

} // FIBEX
} // ASAM
